using VerichatCRM.Web.Models.Auth;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IAdminApiService
    {
        Task<List<UserListItem>> GetUsersAsync();
        Task<CreateUserOutcome> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
        Task<bool> DeactivateUserAsync(string userId);
        Task<List<HelpRequestItem>> GetHelpRequestsAsync();
        Task<HelpOutcome> ResolveHelpRequestAsync(string id);
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

    /// <summary>
    /// What the API said about a create attempt. The sentence matters: "Email is
    /// already registered." is the answer an administrator needs, and a bare
    /// failure would only let the form say that the save did not happen.
    /// </summary>
    public class CreateUserOutcome
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
