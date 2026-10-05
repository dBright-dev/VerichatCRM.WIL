using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using System.Security.Claims;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotations;

        public QuotationController(IQuotationService quotations) => _quotations = quotations;

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
