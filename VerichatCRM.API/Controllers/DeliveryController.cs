using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryLogService _deliveries;

        public DeliveryController(IDeliveryLogService deliveries) => _deliveries = deliveries;

        private string CurrentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "anonymous";

        [HttpGet]
        [Authorize(Roles = "System Administrator,Field Support Engineer,Sales Representative")]
        public async Task<IActionResult> GetAll() => Ok(await _deliveries.GetAllAsync());

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId) =>
            Ok(await _deliveries.GetByClientAsync(clientId));

        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            var d = await _deliveries.GetByTrackingAsync(trackingNumber);
            return d == null ? NotFound() : Ok(d);
        }

        [HttpPatch("{deliveryId}/status")]
        [Authorize(Roles = "System Administrator,Field Support Engineer")]
        public async Task<IActionResult> UpdateStatus(string deliveryId, [FromBody] UpdateDeliveryStatusRequest request)
        {
            var d = await _deliveries.UpdateStatusAsync(deliveryId, request, CurrentUserId);
            return d == null ? NotFound() : Ok(d);
        }
    }
}
