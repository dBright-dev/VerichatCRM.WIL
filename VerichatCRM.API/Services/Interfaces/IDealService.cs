using VerichatCRM.API.DTOs.Responses;

namespace VerichatCRM.API.Services.Interfaces
{
    public interface IDealService
    {
        Task<IEnumerable<DealDto>> GetAllAsync();
        Task<IEnumerable<DealDto>> GetByClientAsync(string clientId);
        Task<DealDto?> UpdateStageAsync(string dealId, string newStage);
    }
}
