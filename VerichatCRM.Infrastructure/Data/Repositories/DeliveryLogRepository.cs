using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class DeliveryLogRepository : IDeliveryLogRepository
    {
        private readonly AppDbContext _context;

        public DeliveryLogRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<DeliveryLog>> GetAllAsync() =>
            await _context.DeliveryLogs
                .Include(d => d.StatusHistory)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<DeliveryLog>> GetByClientAsync(string clientId) =>
            await _context.DeliveryLogs
                .Include(d => d.StatusHistory)
                .Where(d => d.ClientId == clientId)
                .ToListAsync();

        public async Task<DeliveryLog?> GetByTrackingAsync(string trackingNumber) =>
            await _context.DeliveryLogs
                .Include(d => d.StatusHistory)
                .FirstOrDefaultAsync(d => d.TrackingNumber == trackingNumber);

        public async Task<DeliveryLog> AddAsync(DeliveryLog log)
        {
            _context.DeliveryLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task UpdateAsync(DeliveryLog log)
        {
            _context.DeliveryLogs.Update(log);
            await _context.SaveChangesAsync();
        }
    }
}
