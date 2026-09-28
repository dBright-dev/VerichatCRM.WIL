using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "System Administrator,Procurement Manager,Sales Representative")]
    public class WarehouseAlertController : ControllerBase
    {
        private readonly IWarehouseAlertService _alertService;

        public WarehouseAlertController(IWarehouseAlertService alertService)
        {
            _alertService = alertService;
        }

        /// <summary>
        /// Get all active (unresolved) warehouse alerts.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WarehouseAlertResponse>>> GetActiveAlerts()
        {
            var alerts = await _alertService.GetActiveAlertsAsync();
            return Ok(alerts);
        }

        /// <summary>
        /// Resolve a warehouse alert.
        /// </summary>
        [HttpPatch("{alertId}/resolve")]
        public async Task<ActionResult<WarehouseAlertResponse>> ResolveAlert(string alertId)
        {
            var alert = await _alertService.ResolveAlertAsync(alertId);
            if (alert == null) return NotFound();
            return Ok(alert);
        }
    }
}
