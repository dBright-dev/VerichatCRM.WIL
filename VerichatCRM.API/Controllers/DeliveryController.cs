using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;

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
        public async Task<IActionResult> GetByClient(string clientId)
        {
            if (!CanAccessClient(clientId)) return Forbid();
            return Ok(await _deliveries.GetByClientAsync(clientId));
        }

        [HttpGet("track/{trackingNumber}")]
        public async Task<IActionResult> Track(string trackingNumber)
        {
            var d = await _deliveries.GetByTrackingAsync(trackingNumber);
            if (d == null) return NotFound();

            // A tracking number identifies an order and reveals a destination
            // address, so it must not be a bearer token: only the owning client
            // (or staff) may resolve it.
            if (!CanAccessClient(d.ClientId)) return Forbid();
            return Ok(d);
        }

        [HttpPatch("{deliveryId}/status")]
        [Authorize(Roles = "System Administrator,Field Support Engineer")]
        public async Task<IActionResult> UpdateStatus(string deliveryId, [FromBody] UpdateDeliveryStatusRequest request)
        {
            var d = await _deliveries.UpdateStatusAsync(deliveryId, request, CurrentUserId);
            return d == null ? NotFound() : Ok(d);
        }

        /// <summary>
        /// Clients are scoped to their own records; staff roles that legitimately
        /// work across deliveries may read any. Enforced here rather than by hiding
        /// links, because the route is reachable directly.
        /// </summary>
        private bool CanAccessClient(string clientId) =>
            User.IsInRole(UserRoles.ClientUser) || User.IsInRole(UserRoles.MedicalStudent)
                ? !string.IsNullOrEmpty(clientId) && clientId == CurrentUserId
                : User.IsInRole(UserRoles.SystemAdministrator)
                  || User.IsInRole(UserRoles.ProcurementManager)
                  || User.IsInRole(UserRoles.Logistics)
                  || User.IsInRole(UserRoles.FieldSupportEngineer)
                  || User.IsInRole(UserRoles.SalesRepresentative);
    }
}
