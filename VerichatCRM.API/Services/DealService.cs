using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;
using VerichatCRM.Core.Models.Pipeline;
using VerichatCRM.Infrastructure.Data;

namespace VerichatCRM.API.Services
{
    public class DealService : IDealService
    {
        private readonly IDealRepository _deals;
        private readonly AppDbContext _db;

        public DealService(IDealRepository deals, AppDbContext db)
        {
            _deals = deals;
            _db = db;
        }

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

            if (string.Equals(deal.Stage, newStage, StringComparison.OrdinalIgnoreCase))
                return Map(deal);

            deal.Stage = newStage;
            await _deals.UpdateAsync(deal);

            var lastContactUtc = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(deal.ClientId))
            {
                var client = await _db.Clients.FindAsync(deal.ClientId);
                if (client != null)
                {
                    client.LastContactDate = lastContactUtc;
                    client.UpdatedAt = lastContactUtc;
                }
            }

            if (!string.IsNullOrWhiteSpace(deal.InquiryId))
            {
                var inquiry = await _db.Inquiries.FindAsync(deal.InquiryId);
                if (inquiry != null)
                {
                    inquiry.LastContactDate = lastContactUtc;
                    inquiry.UpdatedAt = lastContactUtc;
                }
            }

            await _db.SaveChangesAsync();
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
