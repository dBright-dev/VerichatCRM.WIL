using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using System.Security.Claims;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data.External;

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

        public QuotationController(
            IQuotationService quotations,
            IQuotationRepository quotationRepo,
            IClientRepository clientRepo,
            IPdfGenerator pdf)
        {
            _quotations = quotations;
            _quotationRepo = quotationRepo;
            _clientRepo = clientRepo;
            _pdf = pdf;
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

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Create([FromBody] CreateQuotationRequest request)
        {
            var q = await _quotations.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = q.QuotationId }, q);
        }
    }
}
