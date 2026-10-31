using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using System.Security.Claims;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data.External;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using VerichatCRM.API.Hubs;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotations;
        private readonly IQuotationRepository _quotationRepo;
        private readonly IClientRepository _clientRepo;
        private readonly IPdfGenerator _pdf;
        private readonly AppDbContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly ILogger<QuotationController> _logger;

        public QuotationController(
            IQuotationService quotations,
            IQuotationRepository quotationRepo,
            IClientRepository clientRepo,
            IPdfGenerator pdf,
            AppDbContext db,
            IHubContext<NotificationHub> hub,
            ILogger<QuotationController> logger)
        {
            _quotations = quotations;
            _quotationRepo = quotationRepo;
            _clientRepo = clientRepo;
            _pdf = pdf;
            _db = db;
            _hub = hub;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> GetAll() => Ok(await _quotations.GetAllAsync());

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId)
        {
            if (User.IsInRole(UserRoles.ClientUser) && clientId != CurrentUserId()) return Forbid();
            return Ok(await _quotations.GetByClientAsync(clientId));
        }

        // The mobile client only needs its own quotation list. Keep this route
        // separate from the staff-only GET /api/quotation collection endpoint.
        // The caller's ID is used below, so staff can open account activity
        // without receiving the sales team's quotation queue.
        [HttpGet("~/api/mobile/quotations")]
        [Authorize]
        public async Task<IActionResult> GetMyMobileQuotations() =>
            Ok(await _quotations.GetByClientAsync(CurrentUserId()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var q = await _quotations.GetByIdAsync(id);
            if (q != null && User.IsInRole(UserRoles.ClientUser) && q.ClientId != CurrentUserId()) return Forbid();
            return q == null ? NotFound() : Ok(q);
        }

        /// <summary>
        /// Renders the quotation as a branded, print-ready PDF (QuestPDF).
        /// </summary>
        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GetPdf(string id)
        {
            var quotation = await _quotationRepo.GetByIdAsync(id);
            if (quotation == null) return NotFound(new { message = "Quotation not found." });

            if (User.IsInRole(UserRoles.ClientUser) && quotation.ClientId != CurrentUserId()) return Forbid();

            var client = await _clientRepo.GetByIdAsync(quotation.ClientId);
            if (client == null) return NotFound(new { message = "Client for this quotation not found." });

            var pdf = _pdf.GenerateQuotationPdf(quotation, client);
            return File(pdf, "application/pdf", $"{quotation.QuoteReference}.pdf");
        }

        private string CurrentUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value ?? string.Empty;

        public sealed record QuoteRequestLine([System.ComponentModel.DataAnnotations.Required] string Sku,
            [System.ComponentModel.DataAnnotations.Range(1, 10000)] int Quantity);
        public sealed record QuoteRequestBody(
            [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(1)] List<QuoteRequestLine> Lines,
            [System.ComponentModel.DataAnnotations.StringLength(4000)] string? Notes);
        public sealed record QuoteStatusRequest([System.ComponentModel.DataAnnotations.Required] string Status);
        public sealed record QuoteResponseRequest([System.ComponentModel.DataAnnotations.Required] string Status);

        /// <summary>Creates a CRM draft quote from live catalogue prices for the signed-in client.</summary>
        [HttpPost("request")]
        [Authorize(Roles = "Client User")]
        public async Task<IActionResult> RequestQuote([FromBody] QuoteRequestBody request)
        {
            var clientId = CurrentUserId();
            var client = await _db.Clients.SingleOrDefaultAsync(c => c.ClientId == clientId);
            if (client == null) return BadRequest(new { message = "Your CRM client account is not set up yet." });
            var requested = request.Lines.GroupBy(x => x.Sku.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Sku = g.Key, Quantity = g.Sum(x => x.Quantity) }).ToList();
            if (requested.Any(x => x.Quantity < 1 || x.Quantity > 10000))
                return BadRequest(new { message = "Each requested product quantity must be between 1 and 10,000." });
            var skus = requested.Select(x => x.Sku).ToList();
            var products = await _db.Products.AsNoTracking().Where(p => skus.Contains(p.SKU)).ToListAsync();
            if (products.Count != skus.Count)
                return BadRequest(new { message = "One or more products are no longer available in the catalogue." });
            var lines = requested.Select(item =>
            {
                var product = products.Single(p => p.SKU == item.Sku);
                return new QuotationLineItem { SKU = product.SKU, ProductName = product.ProductName,
                    Quantity = item.Quantity, UnitPrice = product.Price };
            }).ToList();
            var subtotal = lines.Sum(x => x.Quantity * x.UnitPrice);
            var deal = await _db.Deals.Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.CreatedAt).FirstOrDefaultAsync();
            if (deal == null)
            {
                deal = new Deal { ClientId = clientId, Stage = "Initial Inquiry", Revenue = subtotal,
                    Notes = "Created from a mobile quote request.", CreatedAt = DateTime.UtcNow };
                _db.Deals.Add(deal);
            }
            var quote = new Quotation
            {
                QuoteReference = "VC-QT-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(),
                ClientId = clientId, Deal = deal, DealId = deal.DealId,
                ValidUntil = DateTime.UtcNow.AddDays(30), VatRate = 0.15m, Subtotal = subtotal,
                VatAmount = subtotal * 0.15m, GrandTotal = subtotal * 1.15m,
                Status = "Draft", Terms = "Mobile quote request. " + (string.IsNullOrWhiteSpace(request.Notes)
                    ? "Please confirm pricing and availability."
                    : "Client notes: " + request.Notes.Trim()),
                LineItems = lines, CreatedAt = DateTime.UtcNow
            };
            _db.Quotations.Add(quote);
            await _db.SaveChangesAsync();
            var dto = await _quotations.GetByIdAsync(quote.QuotationId);
            var notice = new { title = "New Mobile Quote Request", message = $"{client.ClientName} requested a quote ({quote.QuoteReference}).", quotationId = quote.QuotationId, clientId };
            // Persisting a request is authoritative; a disconnected live-notification service must not
            // turn a saved quote into an apparent failed submission and cause duplicate retries.
            try
            {
                await Task.WhenAll(
                    _hub.Clients.Group(UserRoles.SystemAdministrator).SendAsync(NotificationHub.NotificationEvent, notice),
                    _hub.Clients.Group(UserRoles.SalesRepresentative).SendAsync(NotificationHub.NotificationEvent, notice),
                    _hub.Clients.Group(UserRoles.ProcurementManager).SendAsync(NotificationHub.NotificationEvent, notice));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Quote {QuoteReference} was saved, but staff SignalR notification failed.", quote.QuoteReference);
            }
            return CreatedAtAction(nameof(GetById), new { id = quote.QuotationId }, dto);
        }

        /// <summary>Sales/admin staff review a mobile request, then mark the verified offer as sent.</summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager")]
        public async Task<IActionResult> UpdateMobileRequestStatus(string id, QuoteStatusRequest request)
        {
            var quote = await _db.Quotations.SingleOrDefaultAsync(q => q.QuotationId == id);
            if (quote == null) return NotFound(new { message = "Quotation not found." });
            if (!quote.Terms.StartsWith("Mobile quote request", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "This workflow only applies to mobile quote requests." });

            var next = request.Status.Trim();
            var validTransition = (quote.Status == "Draft" && next == "Reviewed")
                || (quote.Status == "Reviewed" && next == "Sent");
            if (!validTransition)
                return Conflict(new { message = "Review the draft first, then send the verified quotation to the client." });

            quote.Status = next;
            quote.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            var dto = await _quotations.GetByIdAsync(id);
            // SignalR alerts connected client sessions; the mobile screen also refreshes on focus/foreground.
            try
            {
                await _hub.Clients.Group("user:" + quote.ClientId).SendAsync(NotificationHub.NotificationEvent,
                    new { title = next == "Sent" ? "Quotation ready" : "Quotation reviewed", message = $"{quote.QuoteReference} is {next.ToLowerInvariant()}.", quotation = dto });
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Quotation {QuoteReference} status was saved, but its client notification failed.", quote.QuoteReference);
            }
            return Ok(dto);
        }

        /// <summary>A client responds only after staff sends the verified quotation.</summary>
        [HttpPatch("~/api/mobile/quotations/{id}/response")]
        [Authorize(Roles = "Client User")]
        public async Task<IActionResult> RespondToMobileQuotation(string id, QuoteResponseRequest request)
        {
            var status = request.Status.Trim();
            if (status is not ("Accepted" or "Rejected"))
                return BadRequest(new { message = "Response must be Accepted or Rejected." });
            var clientId = CurrentUserId();
            var quote = await _db.Quotations.SingleOrDefaultAsync(q => q.QuotationId == id && q.ClientId == clientId);
            if (quote == null) return NotFound(new { message = "Quotation not found for this client account." });
            if (quote.Status != "Sent") return Conflict(new { message = "Only a sent quotation can be accepted or declined." });
            quote.Status = status;
            quote.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            var notice = new { title = $"Quotation {status}", message = $"Client responded to {quote.QuoteReference}: {status}.", quotationId = id, clientId };
            try
            {
                await Task.WhenAll(
                    _hub.Clients.Group(UserRoles.SystemAdministrator).SendAsync(NotificationHub.NotificationEvent, notice),
                    _hub.Clients.Group(UserRoles.SalesRepresentative).SendAsync(NotificationHub.NotificationEvent, notice),
                    _hub.Clients.Group(UserRoles.ProcurementManager).SendAsync(NotificationHub.NotificationEvent, notice));
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Client response to quotation {QuoteReference} was saved, but staff notification failed.", quote.QuoteReference);
            }
            return Ok(await _quotations.GetByIdAsync(id));
        }

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request)
        {
            var q = await _quotations.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = q.QuotationId }, q);
        }
    }
}
