using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data.External;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// Password resets and email confirmations.
    ///
    /// Both flows hand a bearer token to somebody who cannot yet sign in, so the two
    /// rules that shape this class are: the token must be useless once it has been
    /// used, and the endpoint must never admit that an address does or does not have
    /// an account.
    /// </summary>
    public class AccountRecoveryService : IAccountRecoveryService
    {
        // An hour for a credential that replaces a password; a working link sitting in
        // an inbox for a week is a real risk, and nobody needs longer than one sitting
        // to paste a new password.
        private static readonly TimeSpan ResetLinkLife = TimeSpan.FromHours(1);

        // A week for a click that proves nothing but that the mailbox is read - the
        // applicant may be waiting days for an approval before they look again.
        private static readonly TimeSpan ConfirmLinkLife = TimeSpan.FromDays(7);

        private const int MinimumPasswordLength = 8;

        private readonly IUserRepository _users;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountRecoveryService> _logger;

        public AccountRecoveryService(
            IUserRepository users,
            IEmailService email,
            IConfiguration config,
            ILogger<AccountRecoveryService> logger)
        {
            _users = users;
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task<AuthResponse> ForgotPasswordAsync(string email)
        {
            var user = string.IsNullOrWhiteSpace(email)
                ? null
                : await _users.GetByEmailAsync(email.Trim());

            if (user is { IsActive: true })
                await IssueAsync(user, LinkKind.Reset);

            // One sentence for both outcomes, so an anonymous caller cannot use this
            // endpoint to list who holds a Verichat account. The same is true of a
            // deactivated account: saying "that account is disabled" would confirm it
            // exists, and a disabled account cannot be reset from this screen anyway -
            // an employee re-enables it.
            return new AuthResponse
            {
                Success = true,
                Message = "If that address has a Verichat account, a reset link is on its way. " +
                          "The link works once and expires in an hour."
            };
        }

        public async Task<AuthResponse> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _users.GetByResetTokenAsync(RecoveryToken.Hash(token));

            if (user == null)
                return Refused("That reset link is not one we issued. Request a new link and try again.");

            if (IsExpired(user.PasswordResetExpiry))
                return Refused("That reset link has expired. Request a new link and try again.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Trim().Length < MinimumPasswordLength)
                return Refused($"Choose a password of at least {MinimumPasswordLength} characters.");

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            // Burn the token with the password it just set. Re-using it would let a
            // stale browser tab or a forwarded email rewrite the password a second
            // time, after the owner already believes the link is spent.
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            await _users.UpdateAsync(user);

            _logger.LogInformation("Password reset completed for {UserId}.", user.UserId);

            // A reset proves the mailbox, not the application. Somebody whose account
            // is still waiting must not be told to sign in.
            if (UserApproval.IsPending(user.ApprovalStatus))
                return new AuthResponse
                {
                    Success = true,
                    Message = "Your password is set. Your account is still waiting for a Verichat employee " +
                              "to approve it, so sign in once that has happened.",
                    ApprovalStatus = user.ApprovalStatus
                };

            if (UserApproval.IsRejected(user.ApprovalStatus))
                return new AuthResponse
                {
                    Success = true,
                    Message = "Your password is set. An employee reviewed this account's application and did " +
                              "not approve it, so it still cannot sign in.",
                    ApprovalStatus = user.ApprovalStatus
                };

            return new AuthResponse
            {
                Success = true,
                Message = "Your password is set. Sign in with it now.",
                ApprovalStatus = user.ApprovalStatus
            };
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string token)
        {
            var user = await _users.GetByVerificationTokenAsync(RecoveryToken.Hash(token));

            if (user == null)
                return Refused("That confirmation link is not one we issued. Ask for a new one.");

            if (IsExpired(user.EmailVerificationExpiry))
                return Refused("That confirmation link has expired. Ask for a new one.");

            user.EmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationExpiry = null;
            await _users.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = $"This address, {user.Email}, is confirmed. Thank you.",
                ApprovalStatus = user.ApprovalStatus
            };
        }

        public async Task SendConfirmationEmailAsync(string userId)
        {
            var user = await _users.GetByIdAsync(userId);
            if (user is not { IsActive: true } || user.EmailVerified) return;

            await IssueAsync(user, LinkKind.Confirm);
        }

        private enum LinkKind
        {
            Reset,
            Confirm
        }

        /// <summary>
        /// Writes a fresh token and sends its link. A new request replaces the old
        /// token rather than adding a second one, so "I clicked the first email by
        /// mistake" cannot leave two live credentials pointing at one account.
        /// </summary>
        private async Task IssueAsync(User user, LinkKind kind)
        {
            var token = RecoveryToken.Create();
            var tokenHash = RecoveryToken.Hash(token);
            var link = BuildLink(kind, token);

            if (kind == LinkKind.Reset)
            {
                user.PasswordResetToken = tokenHash;
                user.PasswordResetExpiry = DateTime.UtcNow.Add(ResetLinkLife);
            }
            else
            {
                user.EmailVerificationToken = tokenHash;
                user.EmailVerificationExpiry = DateTime.UtcNow.Add(ConfirmLinkLife);
            }

            await _users.UpdateAsync(user);

            // The decision is already stored, so a relay that is down costs the email
            // and not the flow; the caller's answer stays neutral either way.
            try
            {
                if (kind == LinkKind.Reset)
                    await _email.SendPasswordResetEmailAsync(user.Email, user.DisplayName, link);
                else
                    await _email.SendEmailConfirmationAsync(user.Email, user.DisplayName, link);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "A {Kind} link was recorded for {UserId} but the email failed to send.",
                    kind, user.UserId);
            }
        }

        /// <summary>
        /// The web app owns these pages, so the link is built from its origin rather
        /// than this API's - a reset form on an API host would have nowhere to post
        /// back to. Unset origin falls back to the deployed web site listed in the
        /// CORS configuration; a developer running the web app elsewhere overrides it
        /// with <c>Sites__WebBaseUrl</c>.
        /// </summary>
        private string BuildLink(LinkKind kind, string token)
        {
            var baseUrl = _config["Sites:WebBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                baseUrl = "http://verichat-crm-web.runasp.net";

            var path = kind == LinkKind.Reset ? "/Account/ResetPassword" : "/Account/VerifyEmail";
            return baseUrl.TrimEnd('/') + path + "?token=" + token;
        }

        private static bool IsExpired(DateTime? expiry) =>
            expiry is null || expiry.Value < DateTime.UtcNow;

        private static AuthResponse Refused(string message) =>
            new() { Success = false, Message = message };
    }
}
