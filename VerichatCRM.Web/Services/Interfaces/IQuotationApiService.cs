using VerichatCRM.Core.Models.Quotation;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IQuotationApiService
    {
        Task<QuotationViewModel?> GetForClientAsync(string clientId);
        Task<QuotationViewModel?> CreateAsync(QuotationViewModel model);
    }
}
