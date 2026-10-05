using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using Xunit;

namespace VerichatCRM.Tests;

public class OrderServiceTests
{
    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task CreateOrder_UsesDatabasePricesAndCombinesDuplicateSkus()
    {
        await using var db = CreateDb();
        db.Products.Add(new Product { SKU = "MASK", ProductName = "Masks", Price = 12.50m, StockQty = 10 });
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        var order = await service.CreateOrderAsync(new CreateOrderRequest
        {
            ClientId = "client-1",
            LineItems = [new() { SKU = "MASK", Quantity = 2 }, new() { SKU = "MASK", Quantity = 3 }]
        });

        Assert.Equal("client-1", order.ClientId);
        Assert.Equal(62.50m, order.TotalAmount);
        Assert.Single(order.LineItems);
        Assert.Equal(5, order.LineItems[0].Quantity);
        Assert.Equal(5, (await db.Products.FindAsync("MASK"))!.StockQty);
    }

    [Fact]
    public async Task CreateOrder_RejectsInvalidQuantityWithoutSavingOrder()
    {
        await using var db = CreateDb();
        var service = new OrderService(db);
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrderAsync(new CreateOrderRequest
        {
            ClientId = "client-1", LineItems = [new() { SKU = "MASK", Quantity = 0 }]
        }));
        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task CreateOrder_RejectsInsufficientStockWithoutSavingOrder()
    {
        await using var db = CreateDb();
        db.Products.Add(new Product { SKU = "MASK", ProductName = "Masks", Price = 12, StockQty = 1 });
        await db.SaveChangesAsync();
        var service = new OrderService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateOrderAsync(new CreateOrderRequest
        {
            ClientId = "client-1", LineItems = [new() { SKU = "MASK", Quantity = 2 }]
        }));
        Assert.Empty(db.Orders);
        Assert.Equal(1, (await db.Products.FindAsync("MASK"))!.StockQty);
    }
}
