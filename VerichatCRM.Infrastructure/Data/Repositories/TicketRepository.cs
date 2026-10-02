using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository   // ← MUST say "public class"
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Ticket>> GetAllAsync() =>
        await _context.Tickets
            .Include(t => t.Equipment)
            .Include(t => t.Activities)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByClientAsync(string clientId) =>
        await _context.Tickets
            .Include(t => t.Equipment)
            .Where(t => t.ClientId == clientId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByAssigneeAsync(string userId) =>
        await _context.Tickets
            .Where(t => t.AssignedToUserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetSlaBreachesAsync() =>
        await _context.Tickets
            .Where(t => !t.IsSlaBreached
                        && t.SlaDeadline < DateTime.UtcNow
                        && t.Status != "Resolved"
                        && t.Status != "Closed")
            .ToListAsync();

    public async Task<Ticket?> GetByIdAsync(string ticketId) =>
        await _context.Tickets
            .Include(t => t.Equipment)
            .Include(t => t.Activities)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task AddActivityAsync(TicketActivity activity)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Activities)
            .FirstOrDefaultAsync(t => t.TicketId == activity.TicketId);

        if (ticket == null) return;
        ticket.Activities.Add(activity);
        await _context.SaveChangesAsync();
    }
}