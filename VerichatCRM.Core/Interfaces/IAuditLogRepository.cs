using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetRecentAsync(int take = 100);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, string entityId);
        Task<IEnumerable<AuditLog>> GetByUserAsync(string userId, int take = 100);
        Task AddAsync(AuditLog entry);
    }
}