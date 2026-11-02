using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AppDbContext _context;

        public AuditLogRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int take = 100) =>
            await _context.AuditLogs
                .AsNoTracking()
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId) =>
            await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

        public async Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int take = 100) =>
            await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(take)
                .ToListAsync();

        public async Task AddAsync(AuditLog entry)
        {
            _context.AuditLogs.Add(entry);
            await _context.SaveChangesAsync();
        }
    }
}