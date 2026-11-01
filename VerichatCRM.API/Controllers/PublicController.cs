using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// The part of the API a person with no account can reach.
    /// <para>
    /// Everything else on this surface is deliberately not here. The enquiry endpoints
    /// belong to <see cref="InquiryController"/>, which requires a representative's role,
    /// because an enquiry can be assigned, scored and closed - three judgements an
    /// anonymous caller has no business making. This controller exists so the public
    /// contact page can hand somebody over, and for nothing else.
    /// </para>
    /// <para>
    /// There is no class-level <c>[Authorize]</c>, and there must not be one: the API has
    /// no fallback policy, so an endpoint without an attribute is anonymous, which is what
    /// a visitor standing on a marketing page needs. The <c>[AllowAnonymous]</c> on the
    /// action is documentation of intent rather than a switch that changes it.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly IPublicEnquiryService _enquiries;
        private readonly IPublicCatalogService _catalog;

        public PublicController(IPublicEnquiryService enquiries, IPublicCatalogService catalog)
        {
            _enquiries = enquiries;
            _catalog = catalog;
        }

        /// <summary>
        /// The catalogue as a visitor sees it: name, category, SKU and photograph.
        /// <para>
        /// The staff endpoint <c>api/product</c> is a different route behind a different
        /// attribute precisely because it answers with prices, quantities on hand and the
        /// reorder threshold. Those three make a supplier's margin readable by anybody
        /// with a browser, so the gallery asks for a projection instead of a permission.
        /// </para>
        /// </summary>
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<IActionResult> Products([FromQuery] string? category)
        {
            var products = await _catalog.GetProductsAsync(category);
            return Ok(products);
        }

        /// <summary>
        /// Records a public enquiry as a Website enquiry and notifies the sales desk.
        /// <para>
        /// The answer does not reveal whether the address had already been heard from in
        /// any way the caller could probe: the fold message says what happened to the note,
        /// which is the same thing a person re-sending needs to know either way.
        /// </para>
        /// </summary>
        [HttpPost("enquiry")]
        [AllowAnonymous]
        public async Task<IActionResult> Enquiry([FromBody] PublicEnquiryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = FirstValidationMessage() });

            var outcome = await _enquiries.SubmitAsync(request);

            return outcome.Success
                ? Ok(new { success = outcome.Success, message = outcome.Message, reference = outcome.Reference })
                : BadRequest(new { success = outcome.Success, message = outcome.Message });
        }

        /// <summary>
        /// The first reason the model binding found, in the visitor's terms rather than
        /// the framework's. A form that refuses without saying which field is the problem
        /// sends the person back through every one of them.
        /// </summary>
        private string FirstValidationMessage()
        {
            var problem = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

            return problem ?? "The form could not be read. Check the details and send it again.";
        }
    }
}
