using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// Backend for the Expo mobile app (client / on-the-go medical staff).
    /// All identity comes from the JWT — the token owner can only see and
    /// mutate their own rows. Field names are camelCase to match the TS types.
    /// </summary>
    [ApiController]
    [Route("api/mobile")]
    [Authorize]
    public class MobileController : ControllerBase
    {
        private readonly IMobileService _mobile;

        public MobileController(IMobileService mobile) => _mobile = mobile;

        private string UserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard() =>
            Ok(await _mobile.GetDashboardAsync(UserId));

        [HttpGet("devices")]
        public async Task<IActionResult> Devices() =>
            Ok(await _mobile.GetDevicesAsync(UserId));

        [HttpPost("devices")]
        public async Task<IActionResult> RegisterDevice([FromBody] RegisterDeviceRequest request)
        {
            var device = await _mobile.RegisterDeviceAsync(UserId, request.Name, request.Type);
            return CreatedAtAction(nameof(Devices), device);
        }

        [HttpPost("devices/{id}/sync")]
        public async Task<IActionResult> SyncDevice(string id)
        {
            var device = await _mobile.SyncDeviceAsync(UserId, id);
            return device == null ? NotFound(new { message = "Device not found." }) : Ok(device);
        }

        [HttpPost("devices/{id}/reconnect")]
        public async Task<IActionResult> ReconnectDevice(string id)
        {
            var device = await _mobile.ReconnectDeviceAsync(UserId, id);
            return device == null ? NotFound(new { message = "Device not found." }) : Ok(device);
        }

        [HttpDelete("devices/{id}")]
        public async Task<IActionResult> RemoveDevice(string id)
        {
            var ok = await _mobile.RemoveDeviceAsync(UserId, id);
            return ok ? NoContent() : NotFound(new { message = "Device not found." });
        }
    }
}
