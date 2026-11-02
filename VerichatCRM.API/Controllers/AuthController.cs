using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;

namespace VerichatCRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IAccountRecoveryService _recovery;

        public AuthController(IAuthService auth, IAccountRecoveryService recovery)
        {
            _auth = auth;
            _recovery = recovery;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Public sign-up. The account works the moment it is written - the response
        /// carries no token only because the form on the other side of this call
        /// already holds the password and signs the person in with it. Staff roles are
        /// refused here; see <see cref="AuthService.RegisterAsync"/>.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _auth.RegisterAsync(request);

            if (!result.Success || result.User == null)
                return BadRequest(result);

            // Asked for after the account is committed rather than inside the write:
            // the address the person typed is where every later notice goes - the
            // confirmation, a reset - and nobody has yet proved they read mail there.
            // A link sent before the row exists would dangle if the write were rolled
            // back.
            await _recovery.SendConfirmationEmailAsync(result.User.UserId);

            return Ok(result);
        }

        /// <summary>
        /// Starts a password reset. Anonymous, and deliberately as uninformative as
        /// it can be: the same sentence and the same status whether or not the
        /// address has an account. Sign-up is public, so an attacker already has a
        /// cheaper way to test an address than this endpoint.
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _recovery.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }

        /// <summary>
        /// Consumes a reset link. A wrong or spent token is refused with a message the
        /// screen can show; nothing about the account is revealed either way.
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _recovery.ResetPasswordAsync(request.Token, request.NewPassword);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>Consumes an email-confirmation link.</summary>
        [HttpPost("confirm-email")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            var result = await _recovery.ConfirmEmailAsync(request.Token);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Re-sends the confirmation link to the signed-in account's own address, for
        /// somebody whose link expired. Authenticated because an anonymous "send to
        /// this address" is the address-listing oracle the reset flow goes out of its
        /// way to avoid being.
        /// </summary>
        [HttpPost("confirm-email/resend")]
        [Authorize]
        public async Task<IActionResult> ResendConfirmation()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { success = false, message = "No account on this token." });

            await _recovery.SendConfirmationEmailAsync(userId);

            // Neutral on purpose: whether the address is already confirmed, or the
            // send failed, the caller is told the same thing, because this endpoint
            // answers a user about their own account and nothing else can use it.
            return Ok(new
            {
                success = true,
                message = "If this address still needs confirming, a new link is on its way. It expires in 7 days."
            });
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
