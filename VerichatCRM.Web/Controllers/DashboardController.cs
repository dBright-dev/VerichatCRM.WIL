using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.ViewModels;
using VerichatCRM.Core.Models.Dashboard;

namespace VerichatCRM.Web.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return SalesRep();
        }

        public IActionResult Admin()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = "Here's what's happening across your workspace.";
            ViewData["CurrentRole"] = "System Administrator";

            var model = new DashboardViewModel
            {
                RoleLabel = "System Administrator",
                Kpis = new List<KpiCardViewModel>
            {
                new() { Label = "Total Revenue (YTD)", Value = "$2.29M", DeltaLabel = "+12% vs last quarter", BadgeVariant = "success" },
                new() { Label = "Active Clients", Value = "8", BadgeVariant = "accent" },
                new() { Label = "Open Tickets", Value = "4", BadgeVariant = "warning" },
                new() { Label = "Warehouse Alerts", Value = "3", DeltaLabel = "Items below min", BadgeVariant = "danger" },
            },
                RevenueTrend = new List<ChartPointViewModel>
            {
                new() { Label = "Feb", Value = 1.2m },
                new() { Label = "Mar", Value = 1.5m },
                new() { Label = "Apr", Value = 1.6m },
                new() { Label = "May", Value = 1.9m },
                new() { Label = "Jun", Value = 2.05m },
                new() { Label = "Jul", Value = 2.29m },
            },
                RecentActivity = new List<ActivityItemViewModel>
            {
                new() { Title = "New inquiry (INQ-2405-002) from Anglo American", Timestamp = "2 hours ago", BadgeVariant = "accent" },
                new() { Title = "Ticket #4521 resolved — Siemens smoke alarms", Timestamp = "5 hours ago", BadgeVariant = "success" },
                new() { Title = "RFP-2025-047 dispatched to Nexam Holdings", Timestamp = "Yesterday", BadgeVariant = "accent" },
                new() { Title = "Landfill inquiry approved — R 1.2M", Timestamp = "Yesterday", BadgeVariant = "success" },
            }
            };

            return View("Index", model);
        }

        public IActionResult SalesRep()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["Subtitle"] = "Here's your pipeline snapshot for this period.";
            ViewData["CurrentRole"] = "Sales Representative";

            var model = new DashboardViewModel
            {
                RoleLabel = "Sales Representative",
                Kpis = new List<KpiCardViewModel>
            {
                new() { Label = "Total Pipeline", Value = "$7,750,000", BadgeVariant = "accent" },
                new() { Label = "Weighted Forecast", Value = "$3,887,500", BadgeVariant = "success" },
                new() { Label = "Deals This Period", Value = "5", BadgeVariant = "accent" },
                new() { Label = "Avg. Deal Size", Value = "$1,550,000", BadgeVariant = "accent" },
            },
                RevenueTrend = new List<ChartPointViewModel>
            {
                new() { Label = "Feb", Value = 4.1m },
                new() { Label = "Mar", Value = 5.0m },
                new() { Label = "Apr", Value = 5.8m },
                new() { Label = "May", Value = 6.4m },
                new() { Label = "Jun", Value = 7.1m },
                new() { Label = "Jul", Value = 7.75m },
            },
                RecentActivity = new List<ActivityItemViewModel>
            {
                new() { Title = "Moved Nexam Holdings deal to Quotation Sent", Timestamp = "1 hour ago", BadgeVariant = "accent" },
                new() { Title = "Closed Won — Siemens maintenance contract", Timestamp = "3 hours ago", BadgeVariant = "success" },
                new() { Title = "New deal created: Anglo American mining inquiry", Timestamp = "Yesterday", BadgeVariant = "accent" },
            }
            };

            return View("Index", model);
        }
    }
}
