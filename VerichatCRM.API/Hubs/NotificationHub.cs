using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VerichatCRM.API.Hubs
{
    /// <summary>
    /// Pushes real-time alerts (SLA breaches, ticket assignments) to the browser.
    /// Every connected client joins a group named after its role, so the server
    /// can address e.g. "System Administrator" or "Field Support Engineer".
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public const string NotificationEvent = "notification";

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrWhiteSpace(role))
                await Groups.AddToGroupAsync(Context.ConnectionId, role);

            // A personal group lets us target one user (e.g. the assignee).
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? Context.User?.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, "user:" + userId);

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Joins a notification group. The requested group is checked against the
        /// authenticated principal instead of being trusted: without this, any signed-in
        /// client could join "System Administrator" and receive internal alerts.
        /// Personal groups ("user:id") are accepted only when they name the caller.
        /// </summary>
        public async Task Subscribe(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                throw new HubException("A group name is required.");

            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? Context.User?.FindFirst("sub")?.Value;

            var allowed = group.StartsWith("user:", StringComparison.Ordinal)
                ? group == "user:" + userId
                : Context.User?.IsInRole(group) == true;

            if (!allowed)
                throw new HubException("You are not permitted to subscribe to that group.");

            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }
    }
}
