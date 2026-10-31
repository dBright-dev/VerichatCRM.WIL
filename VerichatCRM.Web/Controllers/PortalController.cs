using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Enums;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Quotation;
using VerichatCRM.Web.Models.Support;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Portal;
using VerichatCRM.Web.ViewModels.Procurement;

namespace VerichatCRM.Web.Controllers
{
    /// <summary>
    /// The self-service screens for the client audience - Client User and
    /// Medical Student. Staff roles never see these pages and client roles never
    /// see a staff screen: the same permission gate that hides the sidebar links
    /// is what refuses the URL, so access is decided on the server.
    /// <para>
    /// Every read here goes through an API route that is already scoped to the
    /// caller's own account, and every write carries the same antiforgery token
    /// the staff forms use. The JWT stays on this server; the browser only ever
    /// sees the rendered page.
    /// </para>
    /// </summary>
    [RequirePermission(Perm.ClientPortal)]
    public class PortalController : Controller
    {
        private readonly IApiClient _api;
        private readonly IProductApiService _products;

        public PortalController(IApiClient api, IProductApiService products)
        {
            _api = api;
            _products = products;
        }

        /// <summary>
        /// The client id is the user id: the Clients row that orders, quotations
        /// and tickets hang off is keyed on it. A session established before this
        /// was stored has no key, so the visitor signs in again and gets a complete one.
        /// </summary>
        private string? ClientId => HttpContext.Session.GetString("UserId");

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        private IActionResult? RequireClient() =>
            ClientId != null && IsAuthenticated ? null : RedirectToAction("Login", "Account");

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole");
        }

        private void Notify(bool ok, string message) =>
            TempData[ok ? "Success" : "Error"] = message;

        public async Task<IActionResult> Index()
        {
            if (RequireClient() is { } denial) return denial;
            var clientId = ClientId!;

            SetHeader("My Verichat", "Your quotations, deliveries and support requests in one place.");

            var vm = new PortalHomeViewModel
            {
                DisplayName = HttpContext.Session.GetString("UserName") ?? string.Empty,
                Quotations = await _api.GetAsync<List<QuotationDto>>("/api/mobile/quotations") ?? new(),
                Tickets = await _api.GetAsync<List<TicketDto>>($"/api/supportticket/client/{clientId}") ?? new(),
                Deliveries = await _api.GetAsync<List<DeliveryDto>>($"/api/delivery/client/{clientId}") ?? new()
            };
            return View(vm);
        }

        public async Task<IActionResult> Catalogue(string? category, string? search)
        {
            if (RequireClient() is { } denial) return denial;

            SetHeader("Catalogue", "Browse the medical supplies catalogue and request a quote.");

            var allProducts = await _products.GetAllAsync();

            // Category chips come from what is actually in the database, so a
            // category only appears once a product exists in it.
            var vm = new PortalCatalogueViewModel();
            foreach (var c in allProducts.Select(p => p.Category)
                         .Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct()
                         .OrderBy(c => c))
            {
                vm.Categories.Add(c);
            }

            var products = allProducts;
            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                products = products.Where(p => p.Category == category).ToList();
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p =>
                    p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            vm.Products = products.Select(p => new ProductCard
            {
                SKU = p.SKU,
                ProductName = p.ProductName,
                Category = p.Category,
                Price = p.Price,
                StockQty = p.StockQty,
                LowStockThreshold = p.LowStockThreshold,
                ImageUrl = p.ImageUrl
            }).ToList();
            vm.ActiveCategory = category ?? "All";
            vm.SearchQuery = search ?? string.Empty;

            return View(vm);
        }

        /// <summary>
        /// Turns the quantities entered on the catalogue page into a draft quotation
        /// on the client's own account. Prices are read from the catalogue by the
        /// API, never from this form: the page shows an indication, and a sales
        /// representative confirms the real offer before it is sent back.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestQuote()
        {
            if (RequireClient() is { } denial) return denial;

            const int maxQuantity = 10_000;
            var lines = new List<object>();
            foreach (var field in Request.Form)
            {
                if (!field.Key.StartsWith("qty-", StringComparison.Ordinal)) continue;

                // A box left blank is not a request; a box holding something the
                // client cannot have is a mistake worth telling them about.
                if (string.IsNullOrWhiteSpace(field.Value)) continue;
                if (!int.TryParse(field.Value, out var quantity))
                {
                    Notify(false, $"{field.Key["qty-".Length..]} is not a whole number of units.");
                    return RedirectToAction(nameof(Catalogue));
                }
                if (quantity > maxQuantity)
                {
                    Notify(false, $"A single request cannot exceed {maxQuantity:N0} units per product.");
                    return RedirectToAction(nameof(Catalogue));
                }
                if (quantity <= 0) continue;

                lines.Add(new { Sku = field.Key["qty-".Length..].Trim(), Quantity = quantity });
            }

            if (lines.Count == 0)
            {
                Notify(false, "Set a quantity on at least one product before requesting a quote.");
                return RedirectToAction(nameof(Catalogue));
            }

            var notes = Request.Form["notes"].ToString().Trim();
            var created = await _api.PostAsync<object, QuotationDto>(
                "/api/quotation/request",
                new { Lines = lines, Notes = string.IsNullOrWhiteSpace(notes) ? null : notes });

            Notify(created != null, created == null
                ? _api.LastError ?? "The quote request could not be sent. Try again in a moment."
                : $"Quote {created!.QuoteReference} requested. A sales representative will confirm pricing and availability.");

            return RedirectToAction(nameof(Quotations));
        }

        public async Task<IActionResult> Quotations(string? status)
        {
            if (RequireClient() is { } denial) return denial;

            SetHeader("My Quotations", "Every quote raised for your account, from request to response.");

            var all = await _api.GetAsync<List<QuotationDto>>("/api/mobile/quotations") ?? new();
            var active = string.IsNullOrWhiteSpace(status) ? "All" : status;
            var vm = new PortalQuotationsViewModel
            {
                // The server decides who may answer a sent quotation; the view only
                // reflects that decision instead of guessing from the role name.
                CanRespond = Perm.Has(HttpContext.Session.GetString("UserRole"), Perm.ClientQuoteRespond),
                ActiveStatus = active,
                Quotations = (active == "All" ? all : all.Where(q => q.Status == active))
                    .OrderByDescending(q => q.DateGenerated)
                    .ToList()
            };

            return View(vm);
        }

        /// <summary>
        /// Accepts or declines a quotation the sales team has already sent. A draft
        /// or reviewed quote has nothing to answer - the API rejects that with a
        /// conflict, and its message is what the client sees.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        [RequirePermission(Perm.ClientQuoteRespond)]
        public async Task<IActionResult> Respond(string id, string decision)
        {
            if (RequireClient() is { } denial) return denial;

            if (decision is not ("Accepted" or "Rejected"))
            {
                Notify(false, "Choose either accept or decline.");
                return RedirectToAction(nameof(Quotations));
            }

            var updated = await _api.PatchAsync<object, QuotationDto>(
                $"/api/mobile/quotations/{id}/response", new { Status = decision });

            Notify(updated != null, updated == null
                ? _api.LastError ?? "That response could not be recorded."
                : $"Quotation {updated!.QuoteReference} was {(decision == "Accepted" ? "accepted" : "declined")}.");

            return RedirectToAction(nameof(Quotations));
        }

        /// <summary>
        /// Streams the branded PDF. The browser cannot fetch it from the API itself,
        /// because only this server holds the token, so the file is pulled here and
        /// handed over.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Pdf(string id)
        {
            if (RequireClient() is { } denial) return denial;

            var file = await _api.GetFileAsync($"/api/quotation/{id}/pdf");
            if (file == null)
            {
                Notify(false, _api.LastError ?? "That quotation could not be produced as a PDF.");
                return RedirectToAction(nameof(Quotations));
            }
            return File(file.Content, file.ContentType, file.FileName);
        }

        public async Task<IActionResult> Tickets()
        {
            if (RequireClient() is { } denial) return denial;

            SetHeader("Support", "Raise a fault or delivery query and follow it through to resolution.");

            var vm = new PortalTicketsViewModel
            {
                Tickets = ((await _api.GetAsync<List<TicketDto>>(
                    $"/api/supportticket/client/{ClientId}") ?? new())
                    .OrderByDescending(t => t.CreatedAt)).ToList()
            };
            ViewData["Priorities"] = TicketPriorities.All;
            return View(vm);
        }

        /// <summary>
        /// Raises a ticket on the signed-in client's own account.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(CreateTicketDto form)
        {
            if (RequireClient() is { } denial) return denial;

            if (string.IsNullOrWhiteSpace(form.Title) || string.IsNullOrWhiteSpace(form.Description))
            {
                Notify(false, "A subject and a description are both required.");
                return RedirectToAction(nameof(Tickets));
            }

            // The client id comes from the session, never from the form: a tampered
            // value would raise a ticket against another organisation.
            form.ClientId = ClientId!;
            form.EquipmentId = string.IsNullOrWhiteSpace(form.EquipmentId) ? null : form.EquipmentId.Trim();

            // Posted through the shared client rather than SupportApiService, whose
            // transient ApiClient carries a different LastError than this request's.
            var created = await _api.PostAsync<CreateTicketDto, TicketDto>("/api/supportticket", form);

            Notify(created != null, created == null
                ? _api.LastError ?? "The support request could not be raised."
                : $"Ticket {created!.TrackingNumber} was raised. We respond within the SLA for its priority.");

            return RedirectToAction(nameof(Tickets));
        }
    }
}
