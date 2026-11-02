using VerichatCRM.Web.Models.Auth;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IWebAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>Asks for a reset link. The answer is the same for an address that
        /// has no account, so this page cannot be used to list registrations.</summary>
        Task<AuthResponse> ForgotPasswordAsync(string email);

        Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request);

        /// <summary>Consumes the token from an email-confirmation link.</summary>
        Task<AuthResponse> ConfirmEmailAsync(string token);

        /// <summary>
        /// Sends an "I cannot get in" note to an administrator. Nothing about the
        /// address is checked first, and nothing about it is reported back.
        /// </summary>
        Task<AuthResponse> AskForHelpAsync(AskForHelpForm form);
    }
}
