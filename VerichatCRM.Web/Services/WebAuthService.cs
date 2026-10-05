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
            var payload = new { email = request.Email, password = request.Password };
            return await _api.PostAsync<object, AuthResponse>("/api/auth/login", payload)
                   ?? new AuthResponse { Success = false, Message = "No response from server." };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            return await _api.PostAsync<RegisterRequest, AuthResponse>("/api/auth/register", request)
                   ?? new AuthResponse { Success = false, Message = "No response from server." };
        }
    }
}
