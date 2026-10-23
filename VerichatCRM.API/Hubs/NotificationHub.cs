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

        /// <summary>Client -> server ping used to prove the socket is live.</summary>
        public Task Subscribe(string role) => Groups.AddToGroupAsync(Context.ConnectionId, role);
    }
}
