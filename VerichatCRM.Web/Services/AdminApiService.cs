using VerichatCRM.Web.Models.Auth;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class AdminApiService : IAdminApiService
    {
        private readonly IApiClient _api;

        public AdminApiService(IApiClient api) => _api = api;

        public async Task<List<UserListItem>> GetUsersAsync() =>
            await _api.GetAsync<List<UserListItem>>("/api/auth/users") ?? new();

        public async Task<CreateUserOutcome> CreateUserAsync(CreateUserRequest request) =>
            await _api.SendForBodyAsync<CreateUserRequest, CreateUserOutcome>(
                HttpMethod.Post, "/api/auth/users", request)
            ?? new CreateUserOutcome
            {
                Message = _api.LastError ?? "The user service could not be reached."
            };

        public async Task<bool> UpdateUserRoleAsync(string userId, string role) =>
            await _api.PutAsync<object, object>($"/api/auth/users/{userId}/role", new { Role = role }) != null;

        public async Task<bool> DeactivateUserAsync(string userId) =>
            await _api.PutAsync<object, object>($"/api/auth/users/{userId}/deactivate", new { }) != null;

        public async Task<List<HelpRequestItem>> GetHelpRequestsAsync() =>
            await _api.GetAsync<List<HelpRequestItem>>("/api/auth/help/open") ?? new();

        public async Task<HelpOutcome> ResolveHelpRequestAsync(string id) =>
            await _api.SendForBodyAsync<object, HelpOutcome>(HttpMethod.Put, $"/api/auth/help/{id}/resolve", new { })
            ?? new HelpOutcome { Message = _api.LastError ?? "The sign-in help service could not be reached." };

        public async Task<bool> RunSlaCheckAsync() =>
            await _api.PostAsync<object, object>("/api/supportticket/run-sla-check", new { }) != null;

        public async Task<bool> ResolveAlertAsync(string alertId) =>
            await _api.PatchAsync<object, object>($"/api/warehousealert/{alertId}/resolve", new { }) != null;
    }
}
