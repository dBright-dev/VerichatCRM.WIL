using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Middleware;
using VerichatCRM.API.Services;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VerichatCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Database
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
    options.UseSqlServer(
        connectionString,
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null))
    .AddInterceptors(sp.GetRequiredService<VerichatCRM.Infrastructure.Data.Interceptors.AuditSaveChangesInterceptor>()));

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS for browser-hosted front ends. Server-side MVC calls do not need CORS,
// but Expo Web does. Production origins are supplied through configuration.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
{
    allowedOrigins = ["https://localhost:7266", "http://localhost:5253", "http://localhost:8081", "http://localhost:8082"];
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


// ---------------------------------------------------------------------------
// Startup data bootstrap: MIGRATE first (this creates the database if it does
// not exist), THEN create the first administrator. Order matters - running
// before migrating throws "Cannot open database ... requested by the login" on
// a fresh machine.
//
// There is deliberately NO demo/seed data. Clients, products, tickets,
// quotations and so on must all be entered by an administrator through the UI.
// Only the initial System Administrator is created, and only from configuration
// - see AdminBootstrapper. Both steps are guarded so an unreachable database
// degrades the app instead of killing the process (which surfaces as HTTP
// 500.30 on IIS).
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
}

app.MapGet("/", () => Results.Redirect("/api/health")).ExcludeFromDescription();
app.MapGet("/health", () => Results.Ok(new { status = "Alive", service = "VerichatCRM.API", timestamp = DateTime.UtcNow })).ExcludeFromDescription();
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("VerichatWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

    // SignalR notification hub
    app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
