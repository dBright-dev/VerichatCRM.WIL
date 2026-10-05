using VerichatCRM.Core.Models.Quotation;
using VerichatCRM.Web.Models.Quotation;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class QuotationApiService : IQuotationApiService
    {
        private readonly IApiClient _api;

        public QuotationApiService(IApiClient api) => _api = api;

        public async Task<QuotationViewModel?> GetForClientAsync(string clientId)
        {
            var dtos = await _api.GetAsync<List<QuotationDto>>($"/api/quotation/client/{clientId}");
            var dto = dtos?.FirstOrDefault();
            return dto == null ? null : Map(dto);
        }

        public async Task<QuotationViewModel?> CreateAsync(QuotationViewModel m)
        {
            var request = new
            {
                DealId = (string?)null,
                ClientId = m.ClientId,
                ValidUntil = m.ValidUntil,
                VatRate = m.VatRate,
                Terms = m.Terms,
                LineItems = m.LineItems.Select(li => new
                {
                    SKU = li.Sku,
                    ProductName = li.ProductName,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice
                })
            };

            var result = await _api.PostAsync<object, QuotationDto>("/api/quotation", request);
            return result == null ? null : Map(result);
        }

        private static QuotationViewModel Map(QuotationDto d) => new()
        {
            QuotationId = d.QuotationId,
            ClientId = d.ClientId,
            ClientName = d.ClientName,
            QuoteReference = d.QuoteReference,
            ValidUntil = d.ValidUntil,
            VatRate = d.VatRate,
            Terms = d.Terms,
            LineItems = d.LineItems.Select((li, i) => new QuotationLineItemViewModel
            {
                RowId = i + 1,
                Sku = li.SKU,
                ProductName = li.ProductName,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice
            }).ToList()
        };
    }
}
