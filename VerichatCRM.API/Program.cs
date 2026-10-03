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

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

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
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IWarehouseAlertService, WarehouseAlertService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IDeliveryLogService, DeliveryLogService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDealService, DealService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();

// Infrastructure Services
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

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

// CORS for the Web app
builder.Services.AddCors(options =>
{
    options.AddPolicy("VerichatWeb", policy =>
        policy.WithOrigins("https://your-web.monsterasp.net", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

// Initialize database migrations from Infrastructure layer.
// Never let a down/unreachable database kill app startup on the host
// (that is exactly what surfaces as HTTP 500.30). Log and continue;
// /api/health will report Degraded until the DB is reachable.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
        await databaseInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed at startup. App will run in degraded mode; check /api/health for details.");
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

app.Run();
