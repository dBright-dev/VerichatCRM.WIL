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
            return await _api.PostAsync<LoginRequest, AuthResponse>("/api/auth/login", request)
                   ?? new AuthResponse { Success = false, Message = "No response from server." };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            return await _api.PostAsync<RegisterRequest, AuthResponse>("/api/auth/register", request)
                   ?? new AuthResponse { Success = false, Message = "No response from server." };
        }
    }
}
