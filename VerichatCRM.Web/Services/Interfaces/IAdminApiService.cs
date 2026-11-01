using VerichatCRM.Web.Models.Auth;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IAdminApiService
    {
        Task<List<UserListItem>> GetUsersAsync();
        Task<UserListItem?> CreateUserAsync(CreateUserRequest request);
        Task<bool> UpdateUserRoleAsync(string userId, string role);
        Task<bool> DeactivateUserAsync(string userId);
        Task<List<PendingAccount>> GetPendingAccountsAsync();
        Task<ApprovalOutcome> ApproveAccountAsync(string userId);
        Task<ApprovalOutcome> RejectAccountAsync(string userId, string reason);
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

        /// <summary>
        /// Pending / Approved / Rejected. A row here is an application until an
        /// employee says otherwise, so the list cannot claim an account is live from
        /// <see cref="IsActive"/> alone - that column has been true since sign-up.
        /// </summary>
        public string ApprovalStatus { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class CreateUserRequest
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// One application in the approval queue, as the reviewer sees it.
    /// </summary>
    public class PendingAccount
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// True when the role they asked for is a staff role rather than a client
        /// one, i.e. when approving them hands out console access. The API computes
        /// it from the same role list the app uses to route a signed-in user.
        /// </summary>
        public bool IsStaffRole { get; set; }

        public int DaysWaiting => Math.Max(0, (int)(DateTime.UtcNow - CreatedAt).TotalDays);
    }

    /// <summary>
    /// What the API said about a decision. A review screen must show why a click did
    /// not take - it was already decided, it was your own application - so the
    /// server's sentence travels with the flag rather than being replaced by a
    /// generic "could not be saved".
    /// </summary>
    public class ApprovalOutcome
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
