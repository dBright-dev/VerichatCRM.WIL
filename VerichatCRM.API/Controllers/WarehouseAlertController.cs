using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WarehouseAlertController : ControllerBase
    {
        private readonly IWarehouseAlertService _alerts;

        public WarehouseAlertController(IWarehouseAlertService alerts) => _alerts = alerts;

        [HttpGet]
        [Authorize(Roles = "System Administrator,Procurement Manager")]
        public async Task<IActionResult> GetActive()
        {
            var alerts = await _alerts.GetActiveAlertsAsync();
            return Ok(alerts);
        }

        [HttpPatch("{id}/resolve")]
        [Authorize(Roles = "System Administrator,Procurement Manager")]
        public async Task<IActionResult> Resolve(string id)
        {
            var alert = await _alerts.ResolveAlertAsync(id);
            if (alert == null) return NotFound(new { message = $"Alert {id} not found." });
            return Ok(alert);
        }
    }
}
