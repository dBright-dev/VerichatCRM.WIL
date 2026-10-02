using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class QuotationRepository : IQuotationRepository
    {
        private readonly AppDbContext _context;

        public QuotationRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<Quotation>> GetAllAsync() =>
            await _context.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .OrderByDescending(q => q.DateGenerated)
                .ToListAsync();

        public async Task<Quotation?> GetByIdAsync(string quotationId) =>
            await _context.Quotations
                .Include(q => q.Client)
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.QuotationId == quotationId);

        public async Task<IEnumerable<Quotation>> GetByClientAsync(string clientId) =>
            await _context.Quotations
                .Include(q => q.LineItems)
                .Where(q => q.ClientId == clientId)
                .ToListAsync();

        public async Task<Quotation> AddAsync(Quotation quotation)
        {
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task UpdateAsync(Quotation quotation)
        {
            quotation.UpdatedAt = DateTime.UtcNow;
            _context.Quotations.Update(quotation);
            await _context.SaveChangesAsync();
        }

        public async Task<string> GenerateNextReferenceAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _context.Quotations
                .CountAsync(q => q.DateGenerated.Year == year);
            return $"VC-QT-{year}-{(count + 1):D4}";
        }
    }
}
