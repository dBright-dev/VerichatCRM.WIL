using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models;
using VerichatCRM.Web.Models.Client;
using VerichatCRM.Web.Models.Inquiry;
using VerichatCRM.Web.Models.Sales;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    // Sales-rep tools: lead tracker and client map.
    public class SalesController : Controller
    {
        private static readonly string[] LeadStages = { "New", "Contacted", "Qualified" };
        private static readonly string[] Filters = { "All", "New", "Contacted", "Qualified", "Due" };

        private readonly IInquiryApiService _inquiries;
        private readonly IClientFieldApiService _field;
        private readonly IWebHostEnvironment _env;

        public SalesController(IInquiryApiService inquiries, IClientFieldApiService field, IWebHostEnvironment env)
        {
            _inquiries = inquiries;
            _field = field;
            _env = env;
        }

        // ---------- Lead tracker ----------

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

            SetHeader("Lead Tracker", "Who to contact next, most urgent first.");
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

        // ---------- Client map (Phase 3) ----------

        [RequirePermission(Perm.SalesMap)]
        public IActionResult Map()
        {
            SetHeader("Client Map", "Where your clients are, and when you last spoke to them.");
            return View();
        }

        // JSON proxy for the map. The browser never calls the API directly.
        [HttpGet]
        [RequirePermission(Perm.SalesMap)]
        public async Task<IActionResult> MapData(bool demo = false)
        {
            if (demo && _env.IsDevelopment()) return Json(DemoMapData());
            return Json(await _field.GetMapDataAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Perm.SalesMap)]
        public async Task<IActionResult> LogVisit([FromBody] LogVisitRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.ClientId))
                return BadRequest(new { success = false });

            var ok = await _field.LogVisitAsync(request.ClientId, request.Note?.Trim());
            return ok ? Ok(new { success = true }) : StatusCode(502, new { success = false });
        }

        public class LogVisitRequest
        {
            public string ClientId { get; set; } = "";
            public string? Note { get; set; }
        }

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        }

        // Development-only sample data so the map can be checked before the API supplies coordinates.
        private static List<MapClientDto> DemoMapData() => new()
        {
            new() { Id = "demo-1", Name = "Demo Mine, Rustenburg",   Lat = -25.667, Lng = 27.242, Sector = "Mine",      Status = "Active",   AssignedRep = "T. Dube",   LastContactDate = DateTime.UtcNow.AddDays(-12) },
            new() { Id = "demo-2", Name = "Demo Hospital, Johannesburg", Lat = -26.204, Lng = 28.047, Sector = "Hospital",  Status = "Active",   AssignedRep = "A. Naidoo", LastContactDate = DateTime.UtcNow.AddDays(-2) },
            new() { Id = "demo-3", Name = "Demo Clinic, Cape Town",  Lat = -33.925, Lng = 18.424, Sector = "Clinic",    Status = "Prospect", AssignedRep = "A. Naidoo", LastContactDate = DateTime.UtcNow.AddDays(-1) },
            new() { Id = "demo-4", Name = "Demo University, Pretoria", Lat = -25.754, Lng = 28.231, Sector = "Education", Status = "Active",   AssignedRep = "T. Dube",   LastContactDate = DateTime.UtcNow.AddDays(-9) },
            new() { Id = "demo-5", Name = "Demo Mine, Polokwane",    Lat = -23.905, Lng = 29.469, Sector = "Mine",      Status = "Prospect", AssignedRep = "T. Dube",   LastContactDate = null },
            new() { Id = "demo-6", Name = "Demo Clinic, no location yet", Sector = "Clinic", Status = "Active", AssignedRep = "A. Naidoo", LastContactDate = DateTime.UtcNow.AddDays(-4) }
        };
    }
}