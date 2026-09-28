<<<<<<< Updated upstream
var builder = WebApplication.CreateBuilder(args);
=======
using FluentValidation;
using FluentValidation.AspNetCore;
using VerichatCRM.API.Middleware;
using VerichatCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.API.Services;
>>>>>>> Stashed changes

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

<<<<<<< Updated upstream
app.UseAuthorization();

app.MapControllers();

app.Run();
=======
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        builder.Services.AddScoped<IWarehouseAlertService, WarehouseAlertService>();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseMiddleware<XssSanitizationMiddleware>();

        app.Run();
    }
}
>>>>>>> Stashed changes
