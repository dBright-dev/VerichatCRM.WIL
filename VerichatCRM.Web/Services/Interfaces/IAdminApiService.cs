using VerichatCRM.Web.Models.Auth;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IAdminApiService
    {
        Task<List<UserListItem>> GetUsersAsync();
        Task<UserListItem?> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> RunSlaCheckAsync();
        Task<bool> ResolveAlertAsync(string alertId);
    }

    public class UserListItem
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
