using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

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
        public async Task<IActionResult> GetByClient(string clientId) =>
            Ok(await _quotations.GetByClientAsync(clientId));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var q = await _quotations.GetByIdAsync(id);
            return q == null ? NotFound() : Ok(q);
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
