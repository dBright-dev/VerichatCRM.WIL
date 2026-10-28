using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.Models.Inquiry;
using VerichatCRM.Web.Models.Sales;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    // Sales-rep tools: lead tracker now, map view in Phase 3.
    public class SalesController : Controller
    {
        private static readonly string[] LeadStages = { "New", "Contacted", "Qualified" };
        private static readonly string[] Filters = { "All", "New", "Contacted", "Qualified", "Due" };

        private readonly IInquiryApiService _inquiries;

        public SalesController(IInquiryApiService inquiries) => _inquiries = inquiries;

        [RequirePermission(Perm.SalesLeads)]
        public async Task<IActionResult> Leads(string? stage)
        {
            var filter = stage is not null && Filters.Contains(stage) ? stage : "All";

            var open = (await _inquiries.GetAllAsync())
                .Where(i => LeadStages.Contains(i.Status))
                .ToList();

            IEnumerable<InquiryViewModel> shown = filter switch
            {
                "All" => open,
                "Due" => open.Where(i => FollowUpHelper.IsFollowUpDue(i.NextFollowUpDate)),
                _ => open.Where(i => i.Status == filter)
            };

            var vm = new LeadsViewModel
            {
                ActiveFilter = filter,
                Filters = Filters.ToList(),
                OpenCount = open.Count,
                DueTodayCount = open.Count(i => i.NextFollowUpDate is not null
                                                && i.NextFollowUpDate.Value.Date == DateTime.UtcNow.Date),
                OverdueCount = open.Count(i => FollowUpHelper.IsFollowUpOverdue(i.NextFollowUpDate)),
                Leads = shown
                    .OrderByDescending(i => FollowUpHelper.IsFollowUpDue(i.NextFollowUpDate))
                    .ThenBy(i => i.NextFollowUpDate ?? DateTime.MaxValue)
                    .ThenByDescending(i => i.Score)
                    .ToList()
            };

            ViewData["Title"] = "Lead Tracker";
            ViewData["Subtitle"] = "Who to contact next, most urgent first.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.SalesLeads)]
        public async Task<IActionResult> MarkContacted(string id, string status, string? filter)
        {
            var ok = await _inquiries.MarkContactedAsync(id, status);
            if (ok) TempData["Success"] = "Lead marked as contacted.";
            else TempData["Error"] = "Couldn't record that contact. Please try again.";
            return RedirectToAction(nameof(Leads), new { stage = filter });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.SalesLeads)]
        public async Task<IActionResult> AddFollowUp(string id, DateTime nextFollowUpDate, string? note, string? filter)
        {
            if (nextFollowUpDate.Date < DateTime.UtcNow.Date)
            {
                TempData["Error"] = "Choose today or a future date for the follow-up.";
                return RedirectToAction(nameof(Leads), new { stage = filter });
            }

            var ok = await _inquiries.AddFollowUpAsync(id, nextFollowUpDate, note?.Trim());
            if (ok) TempData["Success"] = "Follow-up scheduled.";
            else TempData["Error"] = "Couldn't save the follow-up. Please try again.";
            return RedirectToAction(nameof(Leads), new { stage = filter });
        }
    }
}