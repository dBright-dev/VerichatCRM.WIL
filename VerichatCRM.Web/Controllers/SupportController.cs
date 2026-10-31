using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Enums;
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

            var role = HttpContext.Session.GetString("UserRole") ?? "";
            ViewData["Title"] = ticket.TrackingNumber;
            ViewData["Subtitle"] = ticket.Title;
            ViewData["CanUpdate"] = Perm.Has(role, Perm.TicketUpdate);
            ViewData["CanViewClient"] = Perm.Has(role, Perm.ClientProfile);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.TicketUpdate)]
        public async Task<IActionResult> UpdateStatus(string ticketId, string newStatus, string? notes)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            if (!TicketStatuses.All.Contains(newStatus))
            {
                TempData["Error"] = "Choose a valid status.";
                return RedirectToAction(nameof(TicketDetail), new { id = ticketId });
            }

            if (newStatus == TicketStatuses.Resolved && string.IsNullOrWhiteSpace(notes))
            {
                TempData["Error"] = "Add resolution notes before marking a ticket resolved.";
                return RedirectToAction(nameof(TicketDetail), new { id = ticketId });
            }

            var updated = await _support.UpdateTicketStatusAsync(ticketId, newStatus, notes?.Trim());
            if (updated is null) TempData["Error"] = "Couldn't update the ticket. Please try again.";
            else TempData["Success"] = $"Status updated to {newStatus}.";

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
