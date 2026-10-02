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

// Infrastructure Services
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

// Background service
builder.Services.AddHostedService<SlaEscalationService>();

// JWT Auth
var jwtKey = builder.Configuration["Jwt:Key"]!;
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

// Initialize database migrations from Infrastructure layer
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await databaseInitializer.InitializeAsync();
}

app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors("VerichatWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
