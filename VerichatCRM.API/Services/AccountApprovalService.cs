using System.Net;
using Microsoft.Extensions.Logging;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data.External;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// Turns a self-registered account from a claim into an access right, or
    /// refuses it. The rule this class exists to hold is that nobody approves
    /// themselves, and that the applicant is told either way.
    /// </summary>
    public class AccountApprovalService : IAccountApprovalService
    {
        private readonly IUserRepository _users;
        private readonly IEmailService _email;
        private readonly ILogger<AccountApprovalService> _logger;

        public AccountApprovalService(IUserRepository users, IEmailService email, ILogger<AccountApprovalService> logger)
        {
            _users = users;
            _email = email;
            _logger = logger;
        }

        public async Task<List<PendingAccountResponse>> GetPendingAsync()
        {
            var pending = await _users.GetByApprovalStatusAsync(UserApproval.Pending);
            return pending
                .Select(u => new PendingAccountResponse
                {
                    UserId = u.UserId,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    IsStaffRole = !UserRoles.ClientAudience.Contains(u.Role)
                })
                .ToList();
        }

        public async Task<ApprovalDecision> ApproveAsync(string userId, string reviewerUserId)
        {
            var user = await _users.GetByIdAsync(userId);
            if (user == null) return NotFound();
            if (IsSelfReview(user, reviewerUserId)) return SelfReview();
            if (!UserApproval.IsPending(user.ApprovalStatus))
                return AlreadyDecided(user);

            user.ApprovalStatus = UserApproval.Approved;
            user.ReviewedByUserId = reviewerUserId;
            user.ReviewedAt = DateTime.UtcNow;
            // A declined application that is later approved must not still carry the
            // reason it was declined; the login path reads it and would otherwise
            // quote it to someone whose account now works.
            user.RejectionReason = null;
            await _users.UpdateAsync(user);

            await TryNotifyAsync(user, "Your Verichat account is approved",
                $"<p>Your Verichat account ({WebUtility.HtmlEncode(user.Email)}) has been approved as " +
                $"<strong>{WebUtility.HtmlEncode(user.Role)}</strong>.</p>" +
                "<p>You can sign in to the web console and the mobile app with the password you chose.</p>");

            return new ApprovalDecision(true, $"Approved {user.Email}.", user.UserId, user.Role);
        }

        public async Task<ApprovalDecision> RejectAsync(string userId, string reviewerUserId, string? reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return new ApprovalDecision(false, "Say why the application is declined, so the person can act on it.");

            reason = reason.Trim();
            if (reason.Length > 1000)
                return new ApprovalDecision(false, "The reason must be 1000 characters or fewer.");

            var user = await _users.GetByIdAsync(userId);
            if (user == null) return NotFound();
            if (IsSelfReview(user, reviewerUserId)) return SelfReview();
            if (!UserApproval.IsPending(user.ApprovalStatus))
                return AlreadyDecided(user);

            user.ApprovalStatus = UserApproval.Rejected;
            user.RejectionReason = reason;
            user.ReviewedByUserId = reviewerUserId;
            user.ReviewedAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);

            await TryNotifyAsync(user, "Your Verichat account application",
                "<p>A Verichat employee reviewed your account application and did not approve it.</p>" +
                $"<p><strong>Reason:</strong> {WebUtility.HtmlEncode(reason)}</p>" +
                "<p>If you can address this, sign up again with the corrected details, or reply to this email.</p>");

            return new ApprovalDecision(true, $"Declined {user.Email}.", user.UserId);
        }

        // An approving employee is the one person who must not be able to approve
        // themselves, and the same reviewer could decline their own wayward colleague
        // out of the queue; both are refused on the same check. The API role gate
        // cannot catch it, because a Sales Representative legitimately holds the
        // permission and can legitimately be in the queue.
        private static bool IsSelfReview(User user, string reviewerUserId) =>
            string.Equals(user.UserId, reviewerUserId, StringComparison.OrdinalIgnoreCase);

        private static ApprovalDecision SelfReview() =>
            new(false, "You cannot review your own account application. Ask another employee.");

        private static ApprovalDecision NotFound() =>
            new(false, "That application is no longer in the database.");

        private static ApprovalDecision AlreadyDecided(User user) =>
            new(false, $"{user.Email} was already {user.ApprovalStatus.ToLowerInvariant()}.");

        /// <summary>
        /// Sends the decision to the applicant without letting the send decide the
        /// outcome. The row is written first and an SMTP failure is logged, because
        /// losing the approval would be worse than the applicant waiting for the
        /// email - and when no SMTP host is configured at all the service logs the
        /// message instead, which is the development case.
        /// </summary>
        private async Task TryNotifyAsync(User user, string subject, string htmlBody)
        {
            try
            {
                await _email.SendAsync(user.Email, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Account decision for {UserId} was recorded but the notice email failed.", user.UserId);
            }
        }
    }
}
