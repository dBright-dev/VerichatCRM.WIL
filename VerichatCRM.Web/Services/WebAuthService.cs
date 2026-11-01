using VerichatCRM.Web.Models.Auth;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class WebAuthService : IWebAuthService
    {
        private readonly IApiClient _api;

        public WebAuthService(IApiClient api)
        {
            _api = api;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // API contract is { email, password } only — never forward the UI role picker.
            // A sign-in the API refuses is still an answer with content: "invalid
            // credentials", "this account is waiting for approval" and "an employee
            // declined this application" are three different screens, and the last two
            // arrive on 401. Reading only 2xx here would flatten all three into
            // "No response from server."
            var payload = new { email = request.Email, password = request.Password };
            return await _api.SendForBodyAsync<object, AuthResponse>(HttpMethod.Post, "/api/auth/login", payload)
                   ?? Refused();
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // Same reason: a refused sign-up says why - that address is already
            // registered, that role cannot be claimed - and the person on the form
            // needs that sentence, not a transport error.
            return await _api.SendForBodyAsync<RegisterRequest, AuthResponse>(HttpMethod.Post, "/api/auth/register", request)
                   ?? Refused();
        }

        public async Task<AuthResponse> ForgotPasswordAsync(string email)
        {
            // Sent as an answer even when the address has no account, so nothing here
            // has to be re-worded for a 404-style refusal - and none is issued.
            var payload = new { email };
            return await _api.SendForBodyAsync<object, AuthResponse>(
                HttpMethod.Post, "/api/auth/forgot-password", payload) ?? Refused();
        }

        public async Task<AuthResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            // ConfirmPassword is this form's concern; the API only ever needs the new
            // one. The token is the whole authority of the request, so it goes as-is.
            var payload = new { token = request.Token, newPassword = request.NewPassword };
            return await _api.SendForBodyAsync<object, AuthResponse>(
                HttpMethod.Post, "/api/auth/reset-password", payload) ?? Refused();
        }

        public async Task<AuthResponse> ConfirmEmailAsync(string token)
        {
            // A spent or forged link comes back as a 400 with the reason - expired,
            // or not one we issued - and the screen has to show that rather than a
            // generic failure, because the fix is a different action.
            var payload = new { token };
            return await _api.SendForBodyAsync<object, AuthResponse>(
                HttpMethod.Post, "/api/auth/confirm-email", payload) ?? Refused();
        }

        /// <summary>
        /// The API answered with something this client could not read as an auth
        /// response. ApiClient keeps the message it found in the body, or the words
        /// for the status code, so the form still says something specific.
        /// </summary>
        private AuthResponse Refused() => new()
        {
            Success = false,
            Message = _api.LastError ?? "The sign-in service could not be reached. Try again in a moment."
        };
    }
}
