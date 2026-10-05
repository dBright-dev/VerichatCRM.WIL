using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Inquiry;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    /// <summary>
    /// The sales enquiry queue. Reachable only by roles that hold
    /// <see cref="Perm.InquiryQueue"/> - administrators and sales
    /// representatives. Everyone else gets a 403.
    /// </summary>
    [RequirePermission(Perm.InquiryQueue)]
    public class InquiryController : Controller
    {
        private static readonly string[] AllStatuses =
            { "New", "Contacted", "Qualified", "Quoted", "Won", "Lost" };

        private static readonly string[] AllSources =
            { "Phone", "Email", "Website", "Walk-in", "Mobile App", "Referral" };

        private readonly IInquiryApiService _inquiries;

        public InquiryController(IInquiryApiService inquiries)
        {
            _inquiries = inquiries;
        }

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
        }

        public async Task<IActionResult> Index(string? status)
        {
            SetHeader("Inquiry Queue", "Every inbound enquiry, scored and worked top-down.");

            var inquiries = await _inquiries.GetAllAsync(status);
            var summary = await _inquiries.GetSummaryAsync();

            var vm = new InquiryQueueViewModel
            {
                Inquiries = inquiries,
                Summary = summary,
                ActiveStatus = string.IsNullOrWhiteSpace(status) ? "All" : status,
            };
            vm.Statuses.AddRange(AllStatuses);
            vm.Sources.AddRange(AllSources);

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create() => Form(new InquiryFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InquiryFormViewModel model)
        {
            if (!ModelState.IsValid) return Form(model);

            var created = await _inquiries.CreateAsync(model);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty,
                    "The enquiry could not be saved. Check the details and try again.");
                return Form(model);
            }

            TempData["Success"] = $"Enquiry \"{created.Subject}\" was added to the queue.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var inquiries = await _inquiries.GetAllAsync();
            var inquiry = inquiries.FirstOrDefault(i => i.InquiryId == id);
            if (inquiry == null) return NotFound();

            return Form(new InquiryFormViewModel
            {
                InquiryId = inquiry.InquiryId,
                Subject = inquiry.Subject,
                ContactName = inquiry.ContactName,
                ContactEmail = inquiry.ContactEmail,
                ContactPhone = inquiry.ContactPhone,
                Company = inquiry.Company,
                ClientId = inquiry.ClientId,
                Description = inquiry.Description,
                Source = inquiry.Source,
                Status = inquiry.Status,
                Score = inquiry.Score,
                Notes = inquiry.Notes,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, InquiryFormViewModel model)
        {
            model.InquiryId = id;
            if (!ModelState.IsValid) return Form(model);

            var updated = await _inquiries.UpdateAsync(id, model);
            if (updated == null)
            {
                ModelState.AddModelError(string.Empty,
                    "The enquiry could not be updated. Check the details and try again.");
                return Form(model);
            }

            TempData["Success"] = $"\"{updated.Subject}\" was updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _inquiries.DeleteAsync(id);
            TempData["Success"] = "The enquiry was deleted.";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult Form(InquiryFormViewModel model)
        {
            var isEdit = !string.IsNullOrEmpty(model.InquiryId);
            SetHeader(
                isEdit ? "Edit Inquiry" : "New Inquiry",
                "Enquiries are scored 0-100; the highest scores surface first in the queue.");

            ViewData["IsEdit"] = isEdit;
            ViewData["AllStatuses"] = AllStatuses;
            ViewData["AllSources"] = AllSources;
            return View("Form", model);
        }
    }
}