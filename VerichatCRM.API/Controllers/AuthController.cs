using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IMobileService _mobile;

        public AuthController(IAuthService auth, IMobileService mobile)
        {
            _auth = auth;
            _mobile = mobile;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _auth.RegisterAsync(request);
            if (!result.Success || result.User == null) return BadRequest(result);

            // Orders, quotations and the ticket queue are keyed on Clients.ClientId,
            // and for this audience that key is the UserId. Mobile sign-up opens the
            // row inside its own transaction; a web sign-up used to stop short of it,
            // leaving the new client account able to sign in but unable to request a
            // quote. Same rule, applied on both registration paths.
            if (UserRoles.ClientAudience.Contains(request.Role))
                await _mobile.EnsureClientAccountAsync(result.User.UserId);

            return Ok(result);
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value,
                Email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                    ?? User.FindFirst("email")?.Value,
                Name = User.Identity?.Name,
                Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            });
        }
    }
}
