using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.ViewModels.Home;

namespace VerichatCRM.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                IsAuthenticated = HttpContext.Session.GetString("JwtToken") != null,
                UserName = HttpContext.Session.GetString("UserName"),
                UserRole = HttpContext.Session.GetString("UserRole"),
                QuickActions = GetQuickActions(),
                Roles = GetRoles()
            };
            return View(model);
        }

        public IActionResult Privacy() => View();

        // Target of UseExceptionHandler for unhandled exceptions.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View("Error", BuildStatusModel(500));
        }

        // Target of UseStatusCodePagesWithReExecute. Renders a branded page for 404 / 403 / other errors and sends 401 back to the login screen.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusPage(int code = 404)
        {
            // Missing images/css/js: don't render an HTML page into an asset request.
            var original = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (original is not null && Path.HasExtension(original.OriginalPath))
                return StatusCode(code);

            if (code == StatusCodes.Status401Unauthorized)
                return RedirectToAction("Login", "Account");

            if (code < 400 || code > 599) code = 404;
            Response.StatusCode = code;

            return code switch
            {
                404 => View("NotFound", BuildStatusModel(404)),
                403 => View("Unauthorized", BuildStatusModel(403)),
                _ => View("Error", BuildStatusModel(code))
            };
        }

        private StatusPageViewModel BuildStatusModel(int code) =>
            StatusPageViewModel.For(
                code,
                HttpContext.Session.GetString("UserRole"),
                HttpContext.Session.GetString("JwtToken") != null,
                Activity.Current?.Id ?? HttpContext.TraceIdentifier);

        // Every Url below must resolve to a real controller action.
        private static List<QuickAction> GetQuickActions() => new()
        {
            new() { Title = "New Quotation",   Description = "Generate a branded PDF quote",  Icon = "📄", Url = "/quotation",                      Role = "Sales Representative", Color = "cyan" },
            new() { Title = "Log Inquiry",     Description = "Capture and rank a new lead",   Icon = "📊", Url = "/inquiry/create",                 Role = "Sales Representative", Color = "blue" },
            
            // "Place Order" removed: no Orders screen exists yet. Re-add when it ships.
            new() { Title = "Track Delivery",  Description = "Live dispatch & route status",  Icon = "🚚", Url = "/logistics/tracker",              Role = "Procurement Manager",  Color = "orange" },
            new() { Title = "View Tickets",    Description = "Complaints and open issues",    Icon = "🎫", Url = "/support/tickethub",              Role = "Procurement Manager",  Color = "purple" },
            new() { Title = "View Inventory",  Description = "Products and stock levels",     Icon = "📦", Url = "/procurement/productcatalog",     Role = "Procurement Manager",  Color = "red" },
            new() { Title = "Pipeline Board",  Description = "Visual sales progress",         Icon = "📈", Url = "/pipeline",                       Role = "Sales Representative", Color = "indigo" },
            new() { Title = "User Management", Description = "Roles & permissions",           Icon = "👥", Url = "/admin/users",                    Role = "System Administrator", Color = "teal" }
        };

        private static List<RoleCard> GetRoles() => new()
        {
            new()
            {
                Name = "System Administrator",
                Tagline = "Platform Control",
                Description = "Configure roles, monitor system health, manage security rules and global analytics.",
                Icon = "⚙",
                Color = "admin",
                Features = new() { "User & role management", "Global analytics dashboard", "Security rules engine", "System audit logs" },
                DashboardUrl = "/admin/dashboard"
            },
            new()
            {
                Name = "Sales Representative",
                Tagline = "Revenue Engine",
                Description = "Rank inquiries, manage the pipeline, and generate quotes in seconds.",
                Icon = "📈",
                Color = "sales",
                Features = new() { "Ranked inquiry queue", "Visual pipeline board", "One-click PDF quotations", "Revenue forecasting" },
                DashboardUrl = "/pipeline"
            },
            new()
            {
                Name = "Procurement Manager",
                Tagline = "Supply & Logistics",
                Description = "Reorder consumables, track deliveries, and monitor warehouse stock in real time.",
                Icon = "📦",
                Color = "procurement",
                Features = new() { "Self-service reordering", "Logistics & delivery tracking", "Low-stock alerts", "Complaint & ticket hub" },
                DashboardUrl = "/procurement/dashboard"
            },
            new()
            {
                Name = "Field Support Engineer",
                Tagline = "On-Site Resolution",
                Description = "Receive SLA-routed tickets, log service history, and update job status from any device.",
                Icon = "🛠",
                Color = "support",
                Features = new() { "SLA-based ticket routing", "Mobile job updates", "Equipment service history", "Escalation tracking" },
                DashboardUrl = "/support/dashboard"
            }
        };
    }
}