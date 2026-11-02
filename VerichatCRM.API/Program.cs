using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Middleware;
using VerichatCRM.API.Services;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VerichatCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VerichatCRM.Infrastructure.Data.Repositories;
using VerichatCRM.Infrastructure.Services;
using VerichatCRM.Infrastructure.Repositories;
using VerichatCRM.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// The database password is deliberately no longer stored in appsettings.json.
// Fail fast with a clear message when it is missing, rather than surfacing an
// obscure SQL error on the first request.
// Supply it as ConnectionStrings__DefaultConnection, or via user-secrets in
// local development.
// ---------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured. Set the environment " +
        "variable ConnectionStrings__DefaultConnection, or run: dotnet user-secrets set " +
        "\"ConnectionStrings:DefaultConnection\" \"<connection string>\"");
}

// Encrypt=False was committed alongside the real password for years, so a
// missing or hostile default is rejected here instead of silently opening a
// plaintext session to production SQL Server. Omitting Encrypt is fine: the
// SqlClient default is encryption on.
if (!builder.Environment.IsDevelopment() && RequestsNoEncryption(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection sets Encrypt=False. Remove that key, or set " +
        "Encrypt=True; TrustServerCertificate=True. See DEPLOYMENT.md.");
}

static bool RequestsNoEncryption(string sqlConnectionString) =>
    sqlConnectionString
        .Split(';', StringSplitOptions.RemoveEmptyEntries)
        .Select(part => part.Split('=', 2))
        .Any(kv => kv.Length == 2
            && kv[0].Trim().Equals("encrypt", StringComparison.OrdinalIgnoreCase)
            && (kv[1].Trim().Equals("false", StringComparison.OrdinalIgnoreCase)
                || kv[1].Trim().Equals("no", StringComparison.OrdinalIgnoreCase)));

// Database
// PendingModelChangesWarning is logged instead of thrown, and the reason is a
// detail of this repository's history rather than a preference. Nine of the
// nineteen migrations were written by hand (DEPLOYMENT.md 8.2), so
// AppDbContextModelSnapshot.cs does not describe the database the chain
// actually builds: four mapped entities - AuditLog, HealthMetric,
// ClientTimelineNote, Appointment - have tables created by hand-written
// migrations that the snapshot has never seen. EF checks the snapshot before
// it applies anything and, on a mismatch, throws this warning out of
// MigrateAsync() without running a single migration. Left at the default, the
// API therefore cannot create or update a schema on any environment, ever,
// including a fresh deployment where no tables exist yet; startup swallows the
// exception, the app comes up with no database, and every request 500s.
// Downgrading it to a log line lets the migration run and still reports the
// drift. The drift itself is the real defect and is tracked in DEPLOYMENT.md 8;
// repairing the snapshot is deliberately not done here, because scaffolding
// from the current model would also propose the unique indexes and the
// decimal(5,4) VatRate that exist in code but not in any database (8.1).
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null))
    .AddInterceptors(sp.GetRequiredService<VerichatCRM.Infrastructure.Data.Interceptors.AuditSaveChangesInterceptor>())
    .ConfigureWarnings(warnings => warnings.Log(RelationalEventId.PendingModelChangesWarning)));

// Register services
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IDeliveryLogRepository, DeliveryLogRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IDealRepository, DealRepository>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
// Password resets and email confirmations: the two flows a person uses when the
// credentials they have do not work.
builder.Services.AddScoped<IAccountRecoveryService, AccountRecoveryService>();
// The third way in, for the accounts the two above cannot reach: a note to an
// administrator from somebody standing outside the sign-in screen.
builder.Services.AddScoped<ILoginHelpService, LoginHelpService>();
// And the door that faces the other way: a note from somebody who has no account yet and
// wants one, filed as an enquiry and mailed to the sales desk.
builder.Services.AddScoped<IPublicEnquiryService, PublicEnquiryService>();
// The gallery's read side: a projection of the catalogue with the commercial columns cut.
builder.Services.AddScoped<IPublicCatalogService, PublicCatalogService>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IWarehouseAlertService, WarehouseAlertService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDeliveryLogService, DeliveryLogService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();

// Infrastructure Services
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
    builder.Services.AddScoped<VerichatCRM.Infrastructure.Data.External.IPdfGenerator,
                               VerichatCRM.Infrastructure.Data.External.PdfGenerator>();
    builder.Services.AddScoped<VerichatCRM.Infrastructure.Data.External.IEmailService,
                               VerichatCRM.Infrastructure.Data.External.EmailService>();

    // Real-time notifications (SLA escalations, ticket assignment)
    builder.Services.AddSignalR();

    // Audit trail: every Added/Modified/Deleted entity is recorded with the
    // acting user and IP. Secrets (password hash + reset token) are filtered.
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<VerichatCRM.Infrastructure.Data.Interceptors.AuditSaveChangesInterceptor>();
    builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();


// Background service
builder.Services.AddHostedService<SlaEscalationService>();
builder.Services.AddHostedService<FollowUpBackgroundService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "Jwt:Key is missing. Set it in appsettings.json, appsettings.Production.json, or the Jwt__Key environment variable.");
}
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (!context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                    return Task.CompletedTask;

                // SignalR's WebSocket and long-polling transports cannot set an
                // Authorization header, so the token arrives either as the
                // access_token query parameter or in the HttpOnly session cookie.
                // Reading it from the cookie here is what lets the web app stop
                // writing the JWT into page script.
                if (!string.IsNullOrEmpty(context.Token))
                    return Task.CompletedTask;

                var token = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                    return Task.CompletedTask;
                }

                if (context.Request.Cookies.TryGetValue("JwtToken", out var cookieToken)
                    && !string.IsNullOrEmpty(cookieToken))
                {
                    context.Token = cookieToken;
                }

                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
// Registered as a model validator against the service collection, not as a
// controller option: the 11.3.1 overload only accepts IServiceCollection. This
// is what makes the validators in API/Validators run at all - before it they
// were resolved by nobody and every request reached the services unchecked.
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS for browser-hosted front ends. Server-side MVC calls do not need CORS,
// but Expo Web does. Production origins are supplied through configuration.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (builder.Environment.IsDevelopment())
{
    // Extended, not replaced: appsettings.json carries the hosted origins, and a
    // developer's local staff app and Expo Web must be able to reach the hub and
    // the API from their own ports without editing shared config.
    allowedOrigins = allowedOrigins
        .Concat(["https://localhost:7266", "http://localhost:5253", "http://localhost:8081", "http://localhost:8082"])
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
builder.Services.AddCors(options =>
{
    options.AddPolicy("VerichatWeb", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (!app.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    app.Logger.LogWarning(
        "Cors:AllowedOrigins is empty. The staff web app and Expo Web run on a different " +
        "origin, so their credentialed fetch() and SignalR connections will be rejected. " +
        "Set Cors__AllowedOrigins__0=https://<staff-web-origin>.");
}


// ---------------------------------------------------------------------------
// Startup data bootstrap: MIGRATE first (this creates the database if it does
// not exist), THEN create the first administrator. Order matters - running
// before migrating throws "Cannot open database ... requested by the login" on
// a fresh machine.
//
// There is no demo data in a deployment. DataSeeder opens a fresh database with
// four clients and a starter catalogue so the screens are not blank on first run,
// and stops there - tickets, quotations and everything the business actually records
// are entered by a user. The initial System Administrator comes only from
// configuration (see AdminBootstrapper), because an empty database otherwise has no
// way to sign anyone in. DemoUserSeeder then adds one account per role, but only on a
// development host that explicitly asks for it. Each step is guarded so an
// unreachable database degrades the app instead of killing the process (which
// surfaces as HTTP 500.30 on IIS).
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Database migration failed at startup. Verify ConnectionStrings:DefaultConnection. " +
            "App will run in degraded mode — check /api/health for details.");
    }

    try
    {
        await DataSeeder.SeedAsync(context);
        logger.LogInformation("Starter catalogue ready.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Starter catalogue seeding failed at startup. App will run in degraded mode — check /api/health for details.");
    }

    try
    {
        await AdminBootstrapper.BootstrapAsync(context, app.Configuration, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Administrator bootstrap failed at startup. If the user table is empty, " +
            "nobody will be able to sign in - check the Bootstrap__* configuration.");
    }

    try
    {
        // Returns immediately unless this is the Development environment with
        // Bootstrap:DemoUsers=true, so a deployed API never reaches the database
        // through here at all.
        await DemoUserSeeder.SeedAsync(
            context,
            scope.ServiceProvider.GetRequiredService<IMobileService>(),
            app.Configuration,
            app.Environment,
            logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Development sign-in accounts could not be seeded. The system still runs; " +
            "sign up through the UI instead.");
    }
}

app.MapGet("/", () => Results.Redirect("/api/health")).ExcludeFromDescription();
// The controller owns /health (HealthController.Live); this second registration
// made every request to it an AmbiguousMatchException, i.e. HTTP 500 on the one
// endpoint a host's uptime monitor calls.
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("VerichatWeb");
app.UseAuthentication();
app.UseAuthorization();
// Ahead of the controllers, so binding and validation work on markup-free values;
// behind authorization, so a request that was going to be rejected never pays for
// a JSON parse. The HtmlSanitizer package has been referenced since the start but
// nothing ever called it - the "sanitisation" in the validators was a substring
// test on data that reached the database intact.
app.UseMiddleware<RequestBodySanitisationMiddleware>();
app.MapControllers();

    // SignalR notification hub
    app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
