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
    public class AssetController : ControllerBase
    {
        private readonly IAssetService _assets;

        public AssetController(IAssetService assets) => _assets = assets;

        private string CurrentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "anonymous";

        [HttpGet]
        [Authorize(Roles = "System Administrator,Field Support Engineer,Sales Representative")]
        public async Task<IActionResult> GetAll() => Ok(await _assets.GetAllAsync());

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetByClient(string clientId)
        {
            if (User.IsInRole(UserRoles.ClientUser) && clientId != CurrentUserId) return Forbid();
            return Ok(await _assets.GetByClientAsync(clientId));
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Client User")]
        public async Task<IActionResult> GetMine() => Ok(await _assets.GetByClientAsync(CurrentUserId));

        [HttpGet("warranty-expiring")]
        [Authorize(Roles = "System Administrator,Field Support Engineer,Sales Representative")]
        public async Task<IActionResult> GetWarrantyExpiring([FromQuery] int daysAhead = 60) =>
            Ok(await _assets.GetWarrantyExpiringAsync(daysAhead));

        [HttpGet("{assetId}")]
        public async Task<IActionResult> GetById(string assetId)
        {
            var a = await _assets.GetByIdAsync(assetId);
            if (a != null && User.IsInRole(UserRoles.ClientUser) && a.ClientId != CurrentUserId) return Forbid();
            return a == null ? NotFound() : Ok(a);
        }

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
        {
            var asset = await _assets.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { assetId = asset.AssetId }, asset);
        }

        [HttpPost("{assetId}/service-log")]
        [Authorize(Roles = "System Administrator,Field Support Engineer")]
        public async Task<IActionResult> AddServiceLog(string assetId, [FromBody] CreateAssetServiceLogRequest request)
        {
            var log = await _assets.AddServiceLogAsync(assetId, request, CurrentUserId);
            return log == null ? NotFound() : Ok(log);
        }
    }
}
