using VerichatCRM.Core.Models.Pipeline;
using VerichatCRM.Web.Models.Deal;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class PipelineApiService : IPipelineApiService
    {
        private readonly IApiClient _api;

        public PipelineApiService(IApiClient api) => _api = api;

        public async Task<PipelineBoardViewModel> GetBoardAsync()
        {
            var dtos = await _api.GetAsync<List<DealDto>>("/api/pipeline/deals") ?? new();

            var board = new PipelineBoardViewModel
            {
                TotalPipelineValue = dtos.Where(d => d.Stage != "Won" && d.Stage != "Lost").Sum(d => d.Revenue),
                DealsThisPeriod = dtos.Count(d => d.CreatedAt >= DateTime.UtcNow.AddDays(-30)),
                WeightedForecast = dtos.Where(d => d.Stage != "Won" && d.Stage != "Lost")
                                         .Sum(d => d.Revenue * (d.Probability / 100m))
            };

            foreach (var stage in PipelineStages.All)
            {
                board.DealsByStage[stage] = dtos
                    .Where(d => d.Stage == stage)
                    .Select(d => new DealViewModel
                    {
                        Id = d.DealId,
                        ClientName = d.ClientName,
                        Value = d.Revenue,
                        AssignedRep = d.AssignedRep ?? "Unassigned",
                        ExpectedCloseDate = d.ExpectedCloseDate,
                        Stage = d.Stage
                    }).ToList();
            }

            return board;
        }

        public async Task<bool> MoveDealAsync(string dealId, string newStage)
        {
            var result = await _api.PostAsync<object, DealDto>(
                $"/api/pipeline/deals/{dealId}/stage",
                new { NewStage = newStage });
            return result != null;
        }
    }
}
