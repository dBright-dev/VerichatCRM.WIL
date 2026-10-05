using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IQuotationService
    {
        Task<IEnumerable<QuotationDto>> GetAllAsync();
        Task<IEnumerable<QuotationDto>> GetByClientAsync(string clientId);
        Task<QuotationDto?> GetByIdAsync(string quotationId);
        Task<QuotationDto> CreateAsync(CreateQuotationRequest request);
    }
}
