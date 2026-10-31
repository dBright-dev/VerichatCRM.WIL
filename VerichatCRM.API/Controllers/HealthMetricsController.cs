using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    /// <summary>Health metrics captured from paired IoT / health devices.</summary>
    [ApiController]
    [Route("api")]
    [Authorize]
    public class HealthMetricsController : ControllerBase
    {
        private readonly IMobileService _mobile;

        public HealthMetricsController(IMobileService mobile) => _mobile = mobile;

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        /// <summary>
        /// Persists a health metric reading captured from a paired IoT device.
        /// The caller supplies the ClientId + DeviceId so field engineers or
        /// clients can attribute readings to a specific client/device.
        /// </summary>
        [HttpPost("healthmetrics")]
        public async Task<IActionResult> LogHealthMetric([FromBody] LogHealthMetricRequest request)
        {
            var result = await _mobile.LogHealthMetricAsync(UserId, request);
            return Ok(result);
        }
    }
}
