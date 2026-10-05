using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// The sales enquiry queue. Staff who may not see it get a 403; it is not
    /// merely hidden in the navigation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "System Administrator,Sales Representative")]
    public class InquiryController : ControllerBase
    {
        private readonly IInquiryService _inquiries;

        public InquiryController(IInquiryService inquiries)
        {
            _inquiries = inquiries;
        }

        /// <summary>List enquiries, highest score first.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InquiryResponse>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? source = null,
            [FromQuery] int? minScore = null)
        {
            return Ok(await _inquiries.GetInquiriesAsync(status, source, minScore));
        }

        /// <summary>Headline counts for the dashboard cards.</summary>
        [HttpGet("summary")]
        public async Task<ActionResult<InquirySummary>> GetSummary()
        {
            return Ok(await _inquiries.GetSummaryAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InquiryResponse>> GetById(string id)
        {
            var inquiry = await _inquiries.GetByIdAsync(id);
            if (inquiry == null) return NotFound(new { message = $"Inquiry {id} not found." });
            return Ok(inquiry);
        }

        [HttpPost]
        public async Task<ActionResult<InquiryResponse>> Create([FromBody] CreateInquiryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var inquiry = await _inquiries.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = inquiry.InquiryId }, inquiry);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<InquiryResponse>> Update(string id, [FromBody] UpdateInquiryRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            InquiryResponse? inquiry;
            try
            {
                inquiry = await _inquiries.UpdateAsync(id, request);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

            if (inquiry == null) return NotFound(new { message = $"Inquiry {id} not found." });
            return Ok(inquiry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return await _inquiries.DeleteAsync(id)
                ? NoContent()
                : NotFound(new { message = $"Inquiry {id} not found." });
        }
    }
}