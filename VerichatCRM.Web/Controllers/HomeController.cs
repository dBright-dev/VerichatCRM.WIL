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

        public IActionResult Error() => View();

        private static List<QuickAction> GetQuickActions() => new()
    {
        new() { Title = "New Quotation",   Description = "Generate a branded PDF quote",  Icon = "📄", Url = "/quotation/create", Role = "Sales Representative",    Color = "cyan" },
        new() { Title = "Log Inquiry",     Description = "Capture and rank a new lead",   Icon = "📊", Url = "/inquiries/create", Role = "Sales Representative",    Color = "blue" },
        new() { Title = "Place Order",     Description = "Reorder medical consumables",   Icon = "🛒", Url = "/orders/create",    Role = "Procurement Manager",    Color = "green" },
        new() { Title = "Track Delivery",  Description = "Live dispatch & route status",  Icon = "🚚", Url = "/logistics",        Role = "Procurement Manager",    Color = "orange" },
        new() { Title = "Open Ticket",     Description = "Log a complaint or issue",      Icon = "🎫", Url = "/tickets/create",   Role = "Procurement Manager",    Color = "purple" },
        new() { Title = "View Inventory",  Description = "Stock levels & alerts",         Icon = "📦", Url = "/warehouse",        Role = "Procurement Manager",    Color = "red" },
        new() { Title = "Pipeline Board",  Description = "Visual sales progress",         Icon = "📈", Url = "/pipeline",         Role = "Sales Representative",    Color = "indigo" },
        new() { Title = "User Management", Description = "Roles & permissions",           Icon = "👥", Url = "/admin/users",      Role = "System Administrator",    Color = "teal" }
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
            DashboardUrl = "/sales/pipeline"
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
            DashboardUrl = "/support/tickets"
        }
    };
    }
}
