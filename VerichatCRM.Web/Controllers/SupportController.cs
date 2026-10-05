using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Support;

namespace VerichatCRM.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly ISupportApiService _support;
        private readonly IApiClient _api;

        public SupportController(ISupportApiService support, IApiClient api) { _support = support; _api = api; }

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        [RequirePermission(Perm.SupportDashboard)]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var dashboard = await _api.GetAsync<VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse>("/api/dashboard/analytics")
                ?? new VerichatCRM.Core.Models.Dashboard.DashboardAnalyticsResponse();
            return View("~/Views/Dashboard/Index.cshtml", dashboard);
        }

        [RequirePermission(Perm.TicketHub)]
        public async Task<IActionResult> TicketHub()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            var tickets = await _support.GetTicketsAsync();
            return View(new TicketHubViewModel { Tickets = tickets });
        }

        [RequirePermission(Perm.TicketHub)]
        public async Task<IActionResult> TicketDetail(string id)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var ticket = await _support.GetTicketAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost]
        [RequirePermission(Perm.TicketHub)]
        public async Task<IActionResult> UpdateStatus(string ticketId, string newStatus, string? notes)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            await _support.UpdateTicketStatusAsync(ticketId, newStatus, notes);
            return RedirectToAction(nameof(TicketDetail), new { id = ticketId });
        }

        private static string GetTimeAgo(DateTime dt)
        {
            var span = DateTime.UtcNow - dt;
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            return $"{(int)span.TotalDays}d ago";
        }
    }
}
