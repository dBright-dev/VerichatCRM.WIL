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

        public SupportController(ISupportApiService support) { _support = support; }

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        [RequirePermission(Perm.SupportDashboard)]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            // This action used to return ~/Views/Dashboard/Index.cshtml — the generic
            // workspace board every role shares — which left Views/Support/Dashboard.cshtml
            // and SupportDashboardViewModel unreferenced, the same way Procurement's board
            // was dead until now. A field engineer signs in to find the tickets that are
            // theirs and which of them have blown their SLA, not order value year to date.
            var tickets = await _support.GetTicketsAsync();
            var assets = await _support.GetAssetsAsync();
            var deliveries = await _support.GetDeliveriesAsync();

            var open = tickets.Where(t => t.Status is not ("Resolved" or "Closed")).ToList();

            var dashboard = new SupportDashboardViewModel
            {
                OpenTickets = open.Count,
                Escalated = open.Count(t => t.Status == "Escalated" || t.EscalationLevel > 0),
                SlaBreaches = open.Count(t => t.IsSlaBreached || t.SlaDeadline < DateTime.UtcNow),
                AssetsUnderMaintenance = assets.Count(a => a.Status == "Needs Service"),
                ActiveShipments = deliveries.Count(d => d.Status is not ("Delivered" or "Cancelled")),
                RecentTickets = open
                    // Longest-open first, not newest: on an SLA board the ticket that
                    // has waited three weeks is the one that needs reading.
                    .OrderBy(t => t.CreatedAt)
                    .Take(8)
                    .Select(t => new RecentTicketItem
                    {
                        TrackingNumber = t.TrackingNumber,
                        Title = t.Title,
                        Priority = t.Priority,
                        Status = t.Status,
                        TimeAgo = TimeAgo(t.CreatedAt)
                    })
                    .ToList()
            };

            ViewData["Title"] = "Support Dashboard";
            ViewData["Subtitle"] = "Open tickets, SLA pressure and what is on the road";
            return View(dashboard);
        }

        // "3h ago", "2d ago" - the queue is measured in hours, so a minute-precise
        // timestamp would be read as more exact than the data actually is.
        private static string TimeAgo(DateTime utc)
        {
            var span = DateTime.UtcNow - utc;
            if (span.TotalMinutes < 1) return "just now";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
            return $"{(int)span.TotalDays / 30}mo ago";
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
