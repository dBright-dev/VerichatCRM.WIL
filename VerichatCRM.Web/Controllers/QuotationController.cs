using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Quotation;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    [RequirePermission(Perm.QuotationBuilder)]
    public class QuotationController : Controller
    {
        private readonly IQuotationApiService _quotations;
        private readonly IClientApiService _clients;
        private readonly IApiClient _api;

        public QuotationController(IQuotationApiService quotations, IClientApiService clients, IApiClient api)
        {
            _quotations = quotations;
            _clients = clients;
            _api = api;
        }

        public async Task<IActionResult> Index(string? clientId)
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Account");

            ViewData["Title"] = "Quotation Builder";
            ViewData["Subtitle"] = "Add line items, review totals, then generate a branded PDF.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";

            var model = new QuotationViewModel();

            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _clients.GetClientAsync(clientId);
                if (client != null)
                {
                    model.ClientId = client.Id;
                    model.ClientName = client.Name;
                    model.ClientSector = client.Sector;
                }

                var existing = await _quotations.GetForClientAsync(clientId);
                if (existing != null)
                {
                    return View(existing);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] QuotationViewModel model)
        {
            if (model == null) return BadRequest();
            var saved = await _quotations.CreateAsync(model);
            return saved == null ? StatusCode(500) : Ok(new { saved.QuotationId, saved.QuoteReference });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AdvanceMobileRequest(string quotationId, string status)
        {
            if (HttpContext.Session.GetString("JwtToken") == null) return RedirectToAction("Login", "Account");
            var updated = await _api.PatchAsync<object, object>(
                $"/api/quotation/{Uri.EscapeDataString(quotationId)}/status", new { status });
            TempData["MobileQuoteMessage"] = updated == null
                ? "The quotation could not be updated. Check your access and the current review stage."
                : status == "Reviewed" ? "Request marked as reviewed." : "Verified quotation sent to the client.";
            return RedirectToAction("Index", "Pipeline");
        }
    }
}
