using VerichatCRM.Web.Models.Auth;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IWebAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
    }
}
