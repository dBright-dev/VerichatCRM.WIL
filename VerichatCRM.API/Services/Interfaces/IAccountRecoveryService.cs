using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    /// <summary>
    /// The two ways an account can be locked out of itself: the person has forgotten
    /// the password, and nobody has yet proved the email address is real. Both are
    /// token flows that leave the system, so both live here rather than in
    /// <see cref="IAuthService"/>, which is about the credentials themselves.
    /// </summary>
    public interface IAccountRecoveryService
    {
        /// <summary>
        /// Starts a password reset. Never says whether the address is registered -
        /// the answer is the same either way, because this endpoint is anonymous and
        /// a public sign-up form already exists to try addresses against.
        /// </summary>
        Task<AuthResponse> ForgotPasswordAsync(string email);

        /// <summary>
        /// Consumes a reset link: sets the new password, invalidates the token, and
        /// says so. A pending application stays pending afterwards - a forgotten
        /// password is not an approval.
        /// </summary>
        Task<AuthResponse> ResetPasswordAsync(string token, string newPassword);

        /// <summary>Consumes an email-confirmation link.</summary>
        Task<AuthResponse> ConfirmEmailAsync(string token);

        /// <summary>
        /// Issues a confirmation token and emails the link. Sent automatically when
        /// an account is created, and again on request by someone who is already
        /// signed in and whose link expired.
        /// </summary>
        Task SendConfirmationEmailAsync(string userId);
    }
}
