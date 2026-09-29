using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Enums;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _tickets;
        private readonly ILogger<TicketService> _logger;

        public TicketService(ITicketRepository tickets, ILogger<TicketService> logger)
        {
            _tickets = tickets;
            _logger = logger;
        }

        public async Task<IEnumerable<TicketResponse>> GetAllAsync() =>
            (await _tickets.GetAllAsync()).Select(Map);

        public async Task<IEnumerable<TicketResponse>> GetByClientAsync(string clientId) =>
            (await _tickets.GetByClientAsync(clientId)).Select(Map);

        public async Task<IEnumerable<TicketResponse>> GetByAssigneeAsync(string userId) =>
            (await _tickets.GetByAssigneeAsync(userId)).Select(Map);

        public async Task<IEnumerable<TicketResponse>> GetActiveEscalationsAsync() =>
            (await _tickets.GetAllAsync())
                .Where(t => t.EscalationLevel > 0 && t.Status != TicketStatuses.Resolved)
                .Select(Map);

        public async Task<TicketResponse?> GetByIdAsync(string ticketId)
        {
            var t = await _tickets.GetByIdAsync(ticketId);
            return t == null ? null : Map(t);
        }

        public async Task<TicketResponse> CreateAsync(CreateTicketRequest request, string userId)
        {
            var slaHours = TicketPriorities.GetSlaHours(request.Priority);

            var ticket = new Ticket
            {
                TrackingNumber = GenerateTrackingNumber(),
                ClientId = request.ClientId,
                EquipmentId = request.EquipmentId,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                PhotoUrls = request.PhotoUrls ?? new List<string>(),
                Priority = request.Priority,
                Status = TicketStatuses.Received,
                Category = request.Category,
                SlaDeadline = DateTime.UtcNow.AddHours(slaHours),
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            ticket.Activities.Add(new TicketActivity
            {
                TicketId = ticket.TicketId,
                Action = "Created",
                ToValue = ticket.Status,
                Notes = $"Ticket logged with {request.Priority} priority. SLA deadline: {ticket.SlaDeadline:u}",
                PerformedByUserId = userId
            });

            await _tickets.AddAsync(ticket);
            _logger.LogInformation("Ticket {TrackingNo} created by {UserId}", ticket.TrackingNumber, userId);
            return Map(ticket);
        }

        public async Task<TicketResponse?> UpdateStatusAsync(string ticketId, UpdateTicketStatusRequest request, string userId)
        {
            var ticket = await _tickets.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            if (!TicketStatuses.All.Contains(request.NewStatus))
                throw new InvalidOperationException($"Invalid status '{request.NewStatus}'.");

            var previous = ticket.Status;
            ticket.Status = request.NewStatus;
            ticket.UpdatedAt = DateTime.UtcNow;

            if (request.NewStatus == TicketStatuses.Resolved || request.NewStatus == TicketStatuses.Closed)
            {
                ticket.ResolvedAt = DateTime.UtcNow;
                ticket.ResolutionNotes = request.ResolutionNotes;
            }

            ticket.Activities.Add(new TicketActivity
            {
                TicketId = ticket.TicketId,
                Action = "StatusChanged",
                FromValue = previous,
                ToValue = request.NewStatus,
                Notes = request.Notes,
                PerformedByUserId = userId
            });

            await _tickets.UpdateAsync(ticket);
            return Map(ticket);
        }

        public async Task<TicketResponse?> AssignAsync(string ticketId, AssignTicketRequest request, string userId)
        {
            var ticket = await _tickets.GetByIdAsync(ticketId);
            if (ticket == null) return null;

            var previous = ticket.AssignedToUserId;
            ticket.AssignedToUserId = request.AssignedToUserId;
            ticket.UpdatedAt = DateTime.UtcNow;

            ticket.Activities.Add(new TicketActivity
            {
                TicketId = ticket.TicketId,
                Action = "Assigned",
                FromValue = previous,
                ToValue = request.AssignedToUserId,
                Notes = request.Notes,
                PerformedByUserId = userId
            });

            await _tickets.UpdateAsync(ticket);
            return Map(ticket);
        }

        /// <summary>
        /// Called by the SLA background service. Escalates tickets that have breached their deadline.
        /// </summary>
        public async Task<int> RunSlaEscalationCheckAsync()
        {
            var breached = (await _tickets.GetSlaBreachesAsync()).ToList();
            var count = 0;

            foreach (var ticket in breached)
            {
                ticket.IsSlaBreached = true;
                ticket.EscalationLevel += 1;
                ticket.EscalatedAt = DateTime.UtcNow;
                ticket.Status = TicketStatuses.Escalated;
                ticket.UpdatedAt = DateTime.UtcNow;

                ticket.Activities.Add(new TicketActivity
                {
                    TicketId = ticket.TicketId,
                    Action = "SlaEscalated",
                    FromValue = ticket.Status,
                    ToValue = TicketStatuses.Escalated,
                    Notes = $"SLA breached. Escalation level {ticket.EscalationLevel}.",
                    PerformedByUserId = "system"
                });

                await _tickets.UpdateAsync(ticket);
                count++;

                _logger.LogWarning("Ticket {TrackingNo} escalated to level {Level}",
                    ticket.TrackingNumber, ticket.EscalationLevel);
            }

            return count;
        }

        private static string GenerateTrackingNumber() =>
            $"VC-TCK-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";

        private static TicketResponse Map(Ticket t) => new()
        {
            TicketId = t.TicketId,
            TrackingNumber = t.TrackingNumber,
            ClientId = t.ClientId,
            EquipmentId = t.EquipmentId,
            AssignedToUserId = t.AssignedToUserId,
            Title = t.Title,
            Description = t.Description,
            PhotoUrls = t.PhotoUrls,
            Priority = t.Priority,
            Status = t.Status,
            Category = t.Category,
            SlaDeadline = t.SlaDeadline,
            IsSlaBreached = t.IsSlaBreached,
            EscalationLevel = t.EscalationLevel,
            CreatedAt = t.CreatedAt,
            ResolvedAt = t.ResolvedAt,
            ResolutionNotes = t.ResolutionNotes,
            Activities = t.Activities
                .OrderBy(a => a.PerformedAt)
                .Select(a => new TicketActivityResponse
                {
                    ActivityId = a.ActivityId,
                    Action = a.Action,
                    FromValue = a.FromValue,
                    ToValue = a.ToValue,
                    Notes = a.Notes,
                    PerformedAt = a.PerformedAt
                }).ToList()
        };
    }
}
