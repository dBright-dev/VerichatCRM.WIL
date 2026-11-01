using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.API.Controllers
{
    /// <summary>
    /// The approval queue for accounts that created themselves.
    /// <para>
    /// Sign-up is public, so a row in <code>Users</code> is an application rather
    /// than a hire. Three roles may decide it: System Administrator always, plus the
    /// Sales Representative and Procurement Manager who can tell a genuine hospital
    /// buyer from someone who just wanted the stock dashboard. Field Support and
    /// Logistics read tickets and dispatch notes, not identities, so they are not on
    /// this route - and neither is the client audience, which cannot approve anyone,
    /// least of all itself.
    /// </para>
    /// <para>
    /// <see cref="UsersController"/> on the same prefix is administrator-only because
    /// it rewrites roles; this one is deliberately wider because it can only ever
    /// grant what the applicant already asked for, and the no-self-approval rule in
    /// the service stops that from being a way to promote yourself.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/auth/approvals")]
    [Authorize(Roles = UserRoles.SystemAdministrator + "," + UserRoles.SalesRepresentative + "," + UserRoles.ProcurementManager)]
    public class AccountApprovalsController : ControllerBase
    {
        private readonly IAccountApprovalService _approvals;
        private readonly IMobileService _mobile;

        public AccountApprovalsController(IAccountApprovalService approvals, IMobileService mobile)
        {
            _approvals = approvals;
            _mobile = mobile;
        }

        private string ReviewerUserId =>
            User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? throw new UnauthorizedAccessException("No user id claim.");

        [HttpGet("pending")]
        public async Task<IActionResult> Pending() => Ok(await _approvals.GetPendingAsync());

        // Both decisions answer with the outcome inside as well as the status code,
        // because the web tier reads the body on a refusal to explain the refusal on
        // the screen - and a reviewer who is told "the request failed" learns nothing.
        [HttpPut("{userId}/approve")]
        public async Task<IActionResult> Approve(string userId)
        {
            var decision = await _approvals.ApproveAsync(userId, ReviewerUserId);
            if (!decision.Success || decision.UserId == null)
                return BadRequest(new { success = false, message = decision.Message });

            // A client's CRM record is keyed on their UserId and is what orders,
            // quotations and the enquiry queue read. It opens here rather than at
            // sign-up because an application is not a client: creating the row when
            // the account was written would put every person who ever filled in the
            // form - approved, declined or abandoned - into the client directory.
            if (decision.Role != null && UserRoles.ClientAudience.Contains(decision.Role))
                await _mobile.EnsureClientAccountAsync(decision.UserId);

            return Ok(new { success = true, message = decision.Message });
        }

        [HttpPut("{userId}/reject")]
        public async Task<IActionResult> Reject(string userId, [FromBody] ReviewAccountRequest request)
        {
            var decision = await _approvals.RejectAsync(userId, ReviewerUserId, request.Reason);
            return decision.Success
                ? Ok(new { success = true, message = decision.Message })
                : BadRequest(new { success = false, message = decision.Message });
        }
    }
}
