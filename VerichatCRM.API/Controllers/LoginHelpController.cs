using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// The way out of a sign-in screen that will not let somebody through.
    /// <para>
    /// <see cref="AuthController"/> on the same prefix answers three problems - a
    /// forgotten password, an unconfirmed address, an application still in the queue -
    /// and each of them assumes the account underneath is basically sound. This route
    /// is for the rest: a declined application they cannot dispute, an address typed
    /// one letter wrong, an account that exists but does not behave. Somebody has to
    /// be able to say that to another human being without first authenticating, which
    /// is the one thing they demonstrably cannot do.
    /// </para>
    /// <para>
    /// The queue is readable by the same three roles that read the approval queue
    /// (<see cref="UserRoles.SystemAdministrator"/>, Sales, Procurement), because they
    /// are the roles that can fix what is reported. It is not open to Field Support or
    /// Logistics: a note about an account is an identity matter, and a ticket handler
    /// reading it could do nothing but forward it.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/auth/help")]
    public class LoginHelpController : ControllerBase
    {
        private readonly ILoginHelpService _help;

        public LoginHelpController(ILoginHelpService help)
        {
            _help = help;
        }

        private string ResolverUserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        /// <summary>
        /// Anonymous, like sign-in itself. The response is the same whether or not the
        /// address is known to the system, because "is this a registered user" is the
        /// question this form is asked to answer and it is not an answer to give out.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Submit([FromBody] AskForHelpRequest request)
        {
            var outcome = await _help.SubmitAsync(request);
            return Ok(new { success = outcome.Success, message = outcome.Message });
        }

        [HttpGet("open")]
        [Authorize(Roles = UserRoles.SystemAdministrator + "," + UserRoles.SalesRepresentative + "," + UserRoles.ProcurementManager)]
        public async Task<IActionResult> Open() => Ok(await _help.GetOpenAsync());

        [HttpPut("{id}/resolve")]
        [Authorize(Roles = UserRoles.SystemAdministrator + "," + UserRoles.SalesRepresentative + "," + UserRoles.ProcurementManager)]
        public async Task<IActionResult> Resolve(string id)
        {
            var outcome = await _help.ResolveAsync(id, ResolverUserId);
            return outcome.Success
                ? Ok(new { success = true, message = outcome.Message })
                : BadRequest(new { success = false, message = outcome.Message });
        }
    }
}
