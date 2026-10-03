using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Quotation;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class QuotationController : Controller
    {
        private readonly IQuotationApiService _quotations;
        private readonly IClientApiService _clients;

        public QuotationController(IQuotationApiService quotations, IClientApiService clients)
        {
            _quotations = quotations;
            _clients = clients;
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
    }
}