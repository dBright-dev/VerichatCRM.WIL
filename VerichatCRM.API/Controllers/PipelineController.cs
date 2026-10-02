using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PipelineController : ControllerBase
    {
        private readonly IDealService _deals;

        public PipelineController(IDealService deals) => _deals = deals;

        [HttpGet("deals")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> GetAll() => Ok(await _deals.GetAllAsync());

        [HttpGet("deals/client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId) =>
            Ok(await _deals.GetByClientAsync(clientId));

        [HttpPatch("deals/{dealId}/stage")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> UpdateStage(string dealId, [FromBody] UpdateDealStageRequest request)
        {
            var d = await _deals.UpdateStageAsync(dealId, request.NewStage);
            return d == null ? NotFound() : Ok(d);
        }
    }
}
