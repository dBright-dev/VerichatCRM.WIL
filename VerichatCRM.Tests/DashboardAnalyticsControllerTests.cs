using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.Controllers;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Infrastructure.Data;
using Xunit;

namespace VerichatCRM.Tests;

public class DashboardAnalyticsControllerTests
{
    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static DashboardAnalyticsController AsRole(AppDbContext db, string role)
    {
        var controller = new DashboardAnalyticsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"))
            }
        };
        return controller;
    }

    [Fact]
    public async Task SalesAnalytics_UsesFullYtdRevenueAndActualInquiryCounts()
    {
        await using var db = CreateDb();
        var now = DateTime.UtcNow;
        db.Orders.AddRange(
            new Order { ClientId = "c1", OrderDate = now.AddMonths(-8), TotalAmount = 120m, Status = "Delivered" },
            new Order { ClientId = "c1", OrderDate = now, TotalAmount = 80m, Status = "Pending" });
        db.Clients.Add(new Client { ClientName = "Active", ContractStatus = "Active" });
        db.Inquiries.AddRange(
            new Inquiry { Subject = "Won", Status = InquiryStatuses.Won },
            new Inquiry { Subject = "New", Status = InquiryStatuses.New });
        await db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(await AsRole(db, UserRoles.SalesRepresentative).Analytics());
        var data = Assert.IsType<DashboardAnalyticsResponse>(result.Value);
        Assert.Contains(data.Kpis, k => k.Label == "Order value year to date" && k.Value == "R 200");
        Assert.Contains(data.Kpis, k => k.Label == "Inquiry conversion" && k.Value == "50%");
        Assert.Contains(data.Kpis, k => k.Label == "Active clients" && k.Value == "1");
        Assert.Equal(80m, data.RevenueTrend[^1].Value);
    }

    [Fact]
    public async Task ProcurementAnalytics_UsesLiveStockOrderAndDeliveryCounts()
    {
        await using var db = CreateDb();
        db.Products.AddRange(
            new Product { SKU = "LOW", ProductName = "Low", StockQty = 2, LowStockThreshold = 5 },
            new Product { SKU = "OK", ProductName = "Okay", StockQty = 9, LowStockThreshold = 5 });
        db.Orders.Add(new Order { ClientId = "c1", Status = "Pending" });
        db.DeliveryLogs.Add(new DeliveryLog { ClientId = "c1", Status = "In Transit" });
        db.DeliveryLogs.Add(new DeliveryLog { ClientId = "c1", Status = "Delivered" });
        await db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(await AsRole(db, UserRoles.ProcurementManager).Analytics());
        var data = Assert.IsType<DashboardAnalyticsResponse>(result.Value);
        Assert.Contains(data.Kpis, k => k.Label == "Products tracked" && k.Value == "2");
        Assert.Contains(data.Kpis, k => k.Label == "Low stock items" && k.Value == "1");
        Assert.Contains(data.Kpis, k => k.Label == "Pending orders" && k.Value == "1");
        Assert.Contains(data.Kpis, k => k.Label == "Active deliveries" && k.Value == "1");
    }

    [Fact]
    public async Task SupportAnalytics_ComputesResolutionTimeAndLiveAssetCounts()
    {
        await using var db = CreateDb();
        var now = DateTime.UtcNow;
        db.Tickets.AddRange(
            new Ticket { TrackingNumber = "T1", Title = "Open", CreatedAt = now.AddHours(-2), Status = "Received" },
            new Ticket { TrackingNumber = "T2", Title = "Resolved", CreatedAt = now.AddHours(-5), ResolvedAt = now.AddHours(-1), Status = "Resolved" });
        db.Assets.Add(new Asset { ClientId = "c1" });
        db.DeliveryLogs.Add(new DeliveryLog { ClientId = "c1", Status = "In Transit" });
        await db.SaveChangesAsync();

        var result = Assert.IsType<OkObjectResult>(await AsRole(db, UserRoles.FieldSupportEngineer).Analytics());
        var data = Assert.IsType<DashboardAnalyticsResponse>(result.Value);
        Assert.Contains(data.Kpis, k => k.Label == "Open tickets" && k.Value == "1");
        Assert.Contains(data.Kpis, k => k.Label == "Avg resolution" && k.Value == "4h");
        Assert.Contains(data.Kpis, k => k.Label == "Assets tracked" && k.Value == "1");
        Assert.Contains(data.Kpis, k => k.Label == "Deliveries active" && k.Value == "1");
    }
}
