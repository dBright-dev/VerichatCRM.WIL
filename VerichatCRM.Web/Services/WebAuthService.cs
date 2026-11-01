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
