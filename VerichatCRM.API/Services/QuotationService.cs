using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotations;

        public QuotationService(IQuotationRepository quotations) => _quotations = quotations;

        public async Task<IEnumerable<QuotationDto>> GetAllAsync() =>
            (await _quotations.GetAllAsync()).Select(Map);

        public async Task<IEnumerable<QuotationDto>> GetByClientAsync(string clientId) =>
            (await _quotations.GetByClientAsync(clientId)).Select(Map);

        public async Task<QuotationDto?> GetByIdAsync(string id)
        {
            var q = await _quotations.GetByIdAsync(id);
            return q == null ? null : Map(q);
        }

        public async Task<QuotationDto> CreateAsync(CreateQuotationRequest r)
        {
            var quotation = new Quotation
            {
                QuoteReference = await _quotations.GenerateNextReferenceAsync(),
                DealId = r.DealId,
                ClientId = r.ClientId,
                ValidUntil = r.ValidUntil ?? DateTime.UtcNow.AddDays(30),
                VatRate = r.VatRate,
                Terms = r.Terms,
                Status = "Draft",
                LineItems = r.LineItems.Select(li => new QuotationLineItem
                {
                    SKU = li.SKU,
                    ProductName = li.ProductName,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice
                }).ToList()
            };

            quotation.Subtotal = quotation.LineItems.Sum(li => li.LineTotal);
            quotation.VatAmount = quotation.Subtotal * quotation.VatRate;
            quotation.GrandTotal = quotation.Subtotal + quotation.VatAmount;

            await _quotations.AddAsync(quotation);
            return Map(quotation);
        }

        private static QuotationDto Map(Quotation q) => new()
        {
            QuotationId = q.QuotationId,
            QuoteReference = q.QuoteReference,
            DealId = q.DealId,
            ClientId = q.ClientId,
            ClientName = q.Client?.ClientName ?? string.Empty,
            DateGenerated = q.DateGenerated,
            ValidUntil = q.ValidUntil,
            Subtotal = q.Subtotal,
            VatRate = q.VatRate,
            VatAmount = q.VatAmount,
            GrandTotal = q.GrandTotal,
            Status = q.Status,
            Terms = q.Terms,
            LineItems = q.LineItems.Select(li => new QuotationLineItemDto
            {
                LineItemId = li.LineItemId,
                SKU = li.SKU,
                ProductName = li.ProductName,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                LineTotal = li.LineTotal
            }).ToList()
        };
    }
}
