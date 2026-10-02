using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Support;

namespace VerichatCRM.Web.Controllers
{
    public class SupportController : Controller
    {
        private readonly ISupportApiService _support;

        public SupportController(ISupportApiService support) => _support = support;

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            var tickets = await _support.GetTicketsAsync();

            var vm = new SupportDashboardViewModel
            {
                OpenTickets = tickets.Count(t => t.Status != "Resolved" && t.Status != "Closed"),
                Escalated = tickets.Count(t => t.EscalationLevel > 0),
                SlaBreaches = tickets.Count(t => t.IsSlaBreached),
                RecentTickets = tickets
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .Select(t => new RecentTicketItem
                    {
                        TrackingNumber = t.TrackingNumber,
                        Title = t.Title,
                        Priority = t.Priority,
                        Status = t.Status,
                        TimeAgo = GetTimeAgo(t.CreatedAt)
                    }).ToList()
            };

            return View(vm);
        }

        public async Task<IActionResult> TicketHub()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            var tickets = await _support.GetTicketsAsync();
            return View(new TicketHubViewModel { Tickets = tickets });
        }

        public async Task<IActionResult> TicketDetail(string id)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var ticket = await _support.GetTicketAsync(id);
            if (ticket == null) return NotFound();
            return View(ticket);
        }

        [HttpPost]
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
