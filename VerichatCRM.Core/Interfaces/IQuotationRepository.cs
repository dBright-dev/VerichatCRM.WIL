using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IQuotationRepository
    {
        Task<IEnumerable<Quotation>> GetAllAsync();
        Task<Quotation?> GetByIdAsync(string quotationId);
        Task<IEnumerable<Quotation>> GetByClientAsync(string clientId);
        Task<Quotation> AddAsync(Quotation quotation);
        Task UpdateAsync(Quotation quotation);
        Task<string> GenerateNextReferenceAsync();
    }
}
