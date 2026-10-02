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
<<<<<<< Updated upstream
=======
using VerichatCRM.Infrastructure.Services;
using VerichatCRM.Infrastructure.Repositories;
>>>>>>> Stashed changes

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
<<<<<<< Updated upstream
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
=======
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
=======
// Infrastructure Services
builder.Services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
builder.Services.AddSwaggerGen();

// CORS for the Web app
builder.Services.AddCors(options =>
{
    options.AddPolicy("VerichatWeb", policy =>
        policy.WithOrigins("https://your-web.monsterasp.net", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
=======
builder.Services.AddOpenApi();
>>>>>>> Stashed changes

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

<<<<<<< Updated upstream
// Apply pending migrations and create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();
        Console.WriteLine("? Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error applying migrations: {ex.Message}");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();
=======
// Initialize database migrations from Infrastructure layer
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>();
    await databaseInitializer.InitializeAsync();
}

app.MapOpenApi();
>>>>>>> Stashed changes
app.UseHttpsRedirection();
app.UseCors("VerichatWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
