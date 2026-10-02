using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class DealRepository : IDealRepository
    {
        private readonly AppDbContext _context;

        public DealRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Deal>> GetAllAsync() =>
            await _context.Deals
                .Include(d => d.Client)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Deal>> GetByClientAsync(string clientId) =>
            await _context.Deals
                .Include(d => d.Client)
                .Where(d => d.ClientId == clientId)
                .ToListAsync();

        public async Task<Deal?> GetByIdAsync(string dealId) =>
            await _context.Deals
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

        public async Task<Deal> AddAsync(Deal deal)
        {
            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();
            return deal;
        }

        public async Task UpdateAsync(Deal deal)
        {
            deal.UpdatedAt = DateTime.UtcNow;
            _context.Deals.Update(deal);
            await _context.SaveChangesAsync();
        }
    }
}
