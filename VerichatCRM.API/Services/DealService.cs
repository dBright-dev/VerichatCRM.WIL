using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Core.Models.Pipeline;

namespace VerichatCRM.API.Services
{
    public class DealService : IDealService
    {
        private readonly IDealRepository _deals;

        public DealService(IDealRepository deals) => _deals = deals;

        public async Task<IEnumerable<DealDto>> GetAllAsync() =>
            (await _deals.GetAllAsync()).Select(Map);

        public async Task<IEnumerable<DealDto>> GetByClientAsync(string clientId) =>
            (await _deals.GetByClientAsync(clientId)).Select(Map);

        public async Task<DealDto?> UpdateStageAsync(string dealId, string newStage)
        {
            if (!PipelineStages.All.Contains(newStage))
                throw new InvalidOperationException($"Invalid stage: {newStage}");

            var deal = await _deals.GetByIdAsync(dealId);
            if (deal == null) return null;

            deal.Stage = newStage;
            await _deals.UpdateAsync(deal);
            return Map(deal);
        }

        private static DealDto Map(Deal d) => new()
        {
            DealId = d.DealId,
            ClientId = d.ClientId,
            ClientName = d.Client?.ClientName ?? string.Empty,
            Stage = d.Stage,
            Revenue = d.Revenue,
            OwnerUserId = d.OwnerUserId,
            AssignedRep = d.OwnerUserId ?? "Unassigned",
            ExpectedCloseDate = d.ExpectedCloseDate,
            Probability = d.Probability,
            CreatedAt = d.CreatedAt
        };
    }
}
