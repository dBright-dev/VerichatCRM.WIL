using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Field Support Engineer,Logistics")]
public sealed class DashboardAnalyticsController(AppDbContext db) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Workspace";
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var orders = await db.Orders.AsNoTracking().Where(o => o.OrderDate >= start).ToListAsync();
        var kpis = new List<KpiCardViewModel>();

        if (role == UserRoles.SalesRepresentative || role == UserRoles.SystemAdministrator)
        {
            var yearStart = new DateTime(now.Year, 1, 1);
            var revenueYtd = await db.Orders.AsNoTracking().Where(o => o.OrderDate >= yearStart && o.Status != "Cancelled").SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
            kpis.Add(new() { Label = "Order value year to date", Value = $"R {revenueYtd:N0}" });
            kpis.Add(new() { Label = "Active clients", Value = (await db.Clients.CountAsync(c => c.ContractStatus == "Active")).ToString() });
            var openInquiries = await db.Inquiries.CountAsync(i => InquiryStatuses.Open.Contains(i.Status));
            kpis.Add(new() { Label = "Open inquiries", Value = openInquiries.ToString() });
            var totalInquiries = await db.Inquiries.CountAsync();
            var won = await db.Inquiries.CountAsync(i => i.Status == InquiryStatuses.Won);
            kpis.Add(new() { Label = "Inquiry conversion", Value = totalInquiries == 0 ? "0%" : $"{won * 100m / totalInquiries:0}%" });
        }
        else if (role == UserRoles.ProcurementManager)
        {
            kpis.Add(new() { Label = "Products tracked", Value = (await db.Products.CountAsync()).ToString() });
            kpis.Add(new() { Label = "Low stock items", Value = (await db.Products.CountAsync(p => p.StockQty <= p.LowStockThreshold)).ToString() });
            kpis.Add(new() { Label = "Pending orders", Value = (await db.Orders.CountAsync(o => o.Status == "Pending" || o.Status == "Processing")).ToString() });
            kpis.Add(new() { Label = "Active deliveries", Value = (await db.DeliveryLogs.CountAsync(d => d.Status != "Delivered" && d.Status != "Cancelled")).ToString() });
        }
        else
        {
            var open = db.Tickets.Where(t => t.Status != "Resolved" && t.Status != "Closed");
            var resolved = await db.Tickets.Where(t => t.ResolvedAt != null && t.ResolvedAt >= now.AddDays(-7)).Select(t => new { t.CreatedAt, t.ResolvedAt }).ToListAsync();
            var avgHours = resolved.Count == 0 ? 0 : resolved.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
            kpis.Add(new() { Label = "Open tickets", Value = (await open.CountAsync()).ToString() });
            kpis.Add(new() { Label = "Avg resolution", Value = $"{avgHours:0.#}h", DeltaLabel = "last 7 days" });
            kpis.Add(new() { Label = "Assets tracked", Value = (await db.Assets.CountAsync()).ToString() });
            kpis.Add(new() { Label = "Deliveries active", Value = (await db.DeliveryLogs.CountAsync(d => d.Status != "Delivered" && d.Status != "Cancelled")).ToString() });
        }

        var trend = Enumerable.Range(0, 6).Select(offset => start.AddMonths(offset)).Select(month => new ChartPointViewModel
        {
            Label = month.ToString("MMM"),
            Value = orders.Where(o => o.OrderDate.Year == month.Year && o.OrderDate.Month == month.Month && o.Status != "Cancelled").Sum(o => o.TotalAmount)
        }).ToList();
        var recentTickets = await db.Tickets.AsNoTracking().OrderByDescending(t => t.CreatedAt).Take(5).ToListAsync();
        var recentOrders = await db.Orders.AsNoTracking().OrderByDescending(o => o.OrderDate).Take(5).ToListAsync();
        var recentClients = await db.Clients.AsNoTracking().OrderByDescending(c => c.CreatedAt).Take(5).ToListAsync();
        var activity = recentTickets.Select(t => (At: t.CreatedAt, Item: new ActivityItemViewModel
            { Title = $"{t.TrackingNumber} — {t.Title}", Timestamp = t.CreatedAt.ToString("HH:mm"), BadgeVariant = t.Priority == "Critical" ? "danger" : "accent" }))
            .Concat(recentOrders.Select(o => (At: o.OrderDate, Item: new ActivityItemViewModel
                { Title = $"Order {o.OrderId} placed — R {o.TotalAmount:N2}", Timestamp = o.OrderDate.ToString("HH:mm"), BadgeVariant = "info" })))
            .Concat(recentClients.Select(c => (At: c.CreatedAt, Item: new ActivityItemViewModel
                { Title = $"New client registered: {c.ClientName}", Timestamp = c.CreatedAt.ToString("HH:mm"), BadgeVariant = "success" })))
            .OrderByDescending(x => x.At).Take(5).Select(x => x.Item).ToList();
        return Ok(new DashboardAnalyticsResponse { RoleLabel = role, Kpis = kpis, RevenueTrend = trend, RecentActivity = activity });
    }
}
