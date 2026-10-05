using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly AppDbContext _context;

        public ClientRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Client>> GetAllAsync() =>
            await _context.Clients
                .OrderBy(c => c.ClientName)
                .ToListAsync();

        public async Task<Client?> GetByIdAsync(string clientId) =>
            await _context.Clients
                .Include(c => c.KeyPersonnel)
                .FirstOrDefaultAsync(c => c.ClientId == clientId);

        public async Task<Client> AddAsync(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task UpdateAsync(Client client)
        {
            client.UpdatedAt = DateTime.UtcNow;
            _context.Clients.Update(client);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string clientId)
        {
            var c = await _context.Clients.FindAsync(clientId);
            if (c == null) return;
            _context.Clients.Remove(c);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalDealsValueAsync(string clientId) =>
            await _context.Deals
                .Where(d => d.ClientId == clientId && d.Stage != "Lost")
                .SumAsync(d => (decimal?)d.Revenue) ?? 0m;
    }
}
