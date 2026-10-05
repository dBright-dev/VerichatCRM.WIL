using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class AdminApiService : IAdminApiService
    {
        private readonly IApiClient _api;

        public AdminApiService(IApiClient api) => _api = api;

        public async Task<List<UserListItem>> GetUsersAsync() =>
            await _api.GetAsync<List<UserListItem>>("/api/auth/users") ?? new();

        public async Task<UserListItem?> CreateUserAsync(CreateUserRequest request) =>
            await _api.PostAsync<CreateUserRequest, UserListItem>("/api/auth/users", request);

        public async Task<bool> UpdateUserRoleAsync(string userId, string role) =>
            await _api.PutAsync<object, object>($"/api/auth/users/{userId}/role", new { Role = role }) != null;

        public async Task<bool> DeactivateUserAsync(string userId) =>
            await _api.PutAsync<object, object>($"/api/auth/users/{userId}/deactivate", new { }) != null;

        public async Task<bool> RunSlaCheckAsync() =>
            await _api.PostAsync<object, object>("/api/supportticket/run-sla-check", new { }) != null;

        public async Task<bool> ResolveAlertAsync(string alertId) =>
            await _api.PatchAsync<object, object>($"/api/warehousealert/{alertId}/resolve", new { }) != null;
    }
}
