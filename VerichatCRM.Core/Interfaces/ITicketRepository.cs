using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Ticket>> GetAllAsync();
        Task<IEnumerable<Ticket>> GetByClientAsync(string clientId);
        Task<IEnumerable<Ticket>> GetByAssigneeAsync(string userId);
        Task<IEnumerable<Ticket>> GetSlaBreachesAsync();
        Task<Ticket?> GetByIdAsync(string ticketId);
        Task<Ticket> AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
        Task AddActivityAsync(TicketActivity activity);
    }
}
