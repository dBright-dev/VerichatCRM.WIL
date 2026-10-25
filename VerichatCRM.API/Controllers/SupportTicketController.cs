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
    public class SupportTicketController : Controller
    {
        private readonly ITicketService _ticketService;
        private readonly IAssetService _assets;

        public SupportTicketController(ITicketService ticket, IAssetService assets)
        {
            _ticketService = ticket;
            _assets = assets;
        }

        private string CurrentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? "anonymous";

        [HttpGet]
        [Authorize(Roles = "System Administrator,Field Support Engineer,Sales Representative")]
        public async Task<IActionResult> GetAll()
            => Ok(await _ticketService.GetAllAsync());

        [HttpGet("client/{clientId}")]
        [Authorize(Roles = "System Administrator,Field Support Engineer,Sales Representative,Client User")]
        public async Task<IActionResult> GetByClient(string clientId)
        {
            if (User.IsInRole(UserRoles.ClientUser) && clientId != CurrentUserId) return Forbid();
            return Ok(await _ticketService.GetByClientAsync(clientId));
        }

        [HttpGet("assigned-to-me")]
        [Authorize(Roles = "Field Support Engineer")]
        public async Task<IActionResult> GetMine()
            => Ok(await _ticketService.GetByAssigneeAsync(CurrentUserId));

        [HttpGet("escalations")]
        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> GetEscalations() =>
        Ok(await _ticketService.GetActiveEscalationsAsync());

        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetById(string ticketId)
        {
            var t = await _ticketService.GetByIdAsync(ticketId);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Client User")]
        public async Task<IActionResult> Create([FromBody] CreateTicketRequest request)
        {
            if (User.IsInRole(UserRoles.ClientUser))
            {
                request.ClientId = CurrentUserId;
                if (!string.IsNullOrWhiteSpace(request.EquipmentId))
                {
                    var asset = await _assets.GetByIdAsync(request.EquipmentId);
                    if (asset == null || asset.ClientId != CurrentUserId) return BadRequest(new { message = "The selected equipment is not registered to your account." });
                }
            }
            var ticket = await _ticketService.CreateAsync(request, CurrentUserId);
            return CreatedAtAction(nameof(GetById), new { ticketId = ticket.TicketId }, ticket);
        }

        [HttpPatch("{ticketId}/status")]
        [Authorize(Roles = "System Administrator,Field Support Engineer")]
        public async Task<IActionResult> UpdateStatus(string ticketId, [FromBody] UpdateTicketStatusRequest request)
        {
            var t = await _ticketService.UpdateStatusAsync(ticketId, request, CurrentUserId);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPatch("{ticketId}/assign")]
        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> Assign(string ticketId, [FromBody] AssignTicketRequest request)
        {
            var t = await _ticketService.AssignAsync(ticketId, request, CurrentUserId);
            return t == null ? NotFound() : Ok(t);
        }

        [HttpPost("run-sla-check")]
        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> RunSlaCheck()
        {
            var count = await _ticketService.RunSlaEscalationCheckAsync();
            return Ok(new { escalated = count });
        }
    }
}
