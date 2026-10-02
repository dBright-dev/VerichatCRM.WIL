using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IDealRepository
    {
        Task<IEnumerable<Deal>> GetAllAsync();
        Task<IEnumerable<Deal>> GetByClientAsync(string clientId);
        Task<Deal?> GetByIdAsync(string dealId);
        Task<Deal> AddAsync(Deal deal);
        Task UpdateAsync(Deal deal);
    }
}
