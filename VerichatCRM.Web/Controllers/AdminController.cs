using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Admin;

namespace VerichatCRM.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IClientApiService _clients;
        private readonly IPipelineApiService _pipeline;
        private readonly ISupportApiService _support;

        public AdminController(
            IClientApiService clients,
            IPipelineApiService pipeline,
            ISupportApiService support)
        {
            _clients = clients;
            _pipeline = pipeline;
            _support = support;
        }

        private bool IsAdmin()
        {
            if (HttpContext.Session.GetString("JwtToken") == null) return false;
            var role = HttpContext.Session.GetString("UserRole");
            return role == "System Administrator";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Account");
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = $"Here's what's happening across your workspace — {DateTime.Now:dddd, d MMMM yyyy}";

            var clients = new List<VerichatCRM.Core.Models.Clients.ClientViewModel>();
            VerichatCRM.Core.Models.Pipeline.PipelineBoardViewModel board = new();
            var tickets = new List<VerichatCRM.Web.Models.Support.TicketDto>();
            var deliveries = new List<VerichatCRM.Web.Models.Support.DeliveryDto>();
            try
            {
                clients = await _clients.GetClientsAsync(null, null);
                board = await _pipeline.GetBoardAsync();
                tickets = await _support.GetTicketsAsync();
                deliveries = await _support.GetDeliveriesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin Dashboard data load failed: {ex}");
            }

            var openTickets = tickets.Where(t => t.Status != "Resolved" && t.Status != "Closed").ToList();
            var monthRevenue = board.TotalPipelineValue;
            var vm = new AdminDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("UserName") ?? "System Administrator",
                CurrentDate = DateTime.Now,
                TotalRevenue = monthRevenue,
                RevenueTarget = 2600000m,
                ActiveClients = clients.Count,
                RenewalsDue = clients.Count(c => c.Status == "Prospect"),
                OpenTickets = openTickets.Count,
                CriticalTickets = openTickets.Count(t => t.Priority == "Critical"),
                LowStockItems = 3,
                RevenueVsTarget = new List<AdminRevenuePoint>
                {
                    new() { Month = "Feb", Revenue = 2000000, Target = 2000000 },
                    new() { Month = "Mar", Revenue = 2050000, Target = 2100000 },
                    new() { Month = "Apr", Revenue = 2150000, Target = 2200000 },
                    new() { Month = "May", Revenue = 2150000, Target = 2300000 },
                    new() { Month = "Jun", Revenue = 2285000, Target = 2450000 },
                    new() { Month = "Jul", Revenue = monthRevenue, Target = 2600000 },
                },
                RecentActivity = openTickets
                    .OrderByDescending(t => t.CreatedAt).Take(3)
                    .Select(t => new AdminActivityItem
                    {
                        Title = $"{t.TrackingNumber} — {t.Title}",
                        Subtitle = t.Status,
                        Timestamp = t.CreatedAt.ToString("HH:mm"),
                        Severity = t.Priority == "Critical" ? "critical" : t.Priority == "High" ? "warning" : "info"
                    })
                    .Concat(deliveries.OrderByDescending(d => d.EstimatedArrival).Take(2)
                        .Select(d => new AdminActivityItem
                        {
                            Title = $"{d.TrackingNumber} dispatched",
                            Subtitle = d.Status,
                            Timestamp = d.EstimatedArrival.ToString("HH:mm"),
                            Severity = "success"
                        }))
                    .Take(5).ToList()
            };
            return View(vm);
        }

        public IActionResult Analytics()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Analytics";
            ViewData["Subtitle"] = "Year-to-date performance overview";
            return View();
        }

        public IActionResult Rules()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Rules Engine";
            ViewData["Subtitle"] = "Security rules and automation";
            return View();
        }

        public IActionResult Users()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "User Management";
            ViewData["Subtitle"] = "Roles & permissions";
            return View();
        }
    }
}
