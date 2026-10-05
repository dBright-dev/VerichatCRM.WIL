using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<TicketResponse>> GetAllAsync();
        Task<IEnumerable<TicketResponse>> GetByClientAsync(string clientId);
        Task<IEnumerable<TicketResponse>> GetByAssigneeAsync(string userId);
        Task<IEnumerable<TicketResponse>> GetActiveEscalationsAsync();
        Task<TicketResponse?> GetByIdAsync(string ticketId);
        Task<TicketResponse> CreateAsync(CreateTicketRequest request, string userId);
        Task<TicketResponse?> UpdateStatusAsync(string ticketId, UpdateTicketStatusRequest request, string userId);
        Task<TicketResponse?> AssignAsync(string ticketId, AssignTicketRequest request, string userId);
        Task<int> RunSlaEscalationCheckAsync();
    }
}
