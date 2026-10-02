using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clients;

        public ClientController(IClientService clients) => _clients = clients;

        [HttpGet]
        [Authorize(Roles = "System Administrator,Sales Representative,Procurement Manager,Field Support Engineer")]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? sector) =>
            Ok(await _clients.GetAllAsync(search, sector));

        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetById(string clientId)
        {
            var c = await _clients.GetByIdAsync(clientId);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpPost]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
        {
            var c = await _clients.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { clientId = c.ClientId }, c);
        }

        [HttpPut("{clientId}")]
        [Authorize(Roles = "System Administrator,Sales Representative")]
        public async Task<IActionResult> Update(string clientId, [FromBody] CreateClientRequest request)
        {
            var c = await _clients.UpdateAsync(clientId, request);
            return c == null ? NotFound() : Ok(c);
        }

        [HttpDelete("{clientId}")]
        [Authorize(Roles = "System Administrator")]
        public async Task<IActionResult> Delete(string clientId)
        {
            await _clients.DeleteAsync(clientId);
            return NoContent();
        }
    }
}
