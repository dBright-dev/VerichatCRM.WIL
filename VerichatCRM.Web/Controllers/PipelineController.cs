using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Pipeline;

namespace VerichatCRM.Web.Controllers
{
    public class PipelineController : Controller
    {
        private static readonly Dictionary<string, decimal> StageWeights = new()
        {
            [PipelineStages.InitialInquiry] = 0.10m,
            [PipelineStages.SiteAssessment] = 0.30m,
            [PipelineStages.ProposalReview] = 0.50m,
            [PipelineStages.QuotationSent] = 0.75m,
            [PipelineStages.ClosedWon] = 1.00m,
        };
        public IActionResult Index()
        {
            ViewData["Title"] = "Sales Pipeline";
            ViewData["Subtitle"] = "Track deals from first inquiry through to close.";
            ViewData["CurrentRole"] = "Sales Representative";

            var deals = GetMockDeals();

            var dealsByStage = PipelineStages.All
                .ToDictionary(
                    stage => stage,
                    stage => deals.Where(d => d.Stage == stage).ToList());

            var model = new PipelineBoardViewModel
            {
                DealsByStage = dealsByStage,
                TotalPipelineValue = deals.Sum(d => d.Value),
                WeightedForecast = deals.Sum(d => d.Value * StageWeights[d.Stage]),
                DealsThisPeriod = deals.Count
            };

            return View(model);
        }

        private static List<DealViewModel> GetMockDeals()
        {
            return new List<DealViewModel>
        {
            new() { Id = 1, ClientName = "Anglo American", Stage = PipelineStages.ProposalReview, Value = 3200000m, AssignedRep = "Thabo N.", ExpectedCloseDate = DateTime.Today.AddDays(14) },
            new() { Id = 2, ClientName = "Nexam Holdings", Stage = PipelineStages.QuotationSent, Value = 1200000m, AssignedRep = "Mamoyo M.", ExpectedCloseDate = DateTime.Today.AddDays(7) },
            new() { Id = 3, ClientName = "Siemens Healthineers", Stage = PipelineStages.ClosedWon, Value = 1550000m, AssignedRep = "Thabo N.", ExpectedCloseDate = DateTime.Today.AddDays(-1) },
            new() { Id = 4, ClientName = "Landfill Solutions SA", Stage = PipelineStages.InitialInquiry, Value = 875000m, AssignedRep = "Mamoyo M.", ExpectedCloseDate = DateTime.Today.AddDays(30) },
            new() { Id = 5, ClientName = "Rand Water Board", Stage = PipelineStages.SiteAssessment, Value = 925000m, AssignedRep = "Thabo N.", ExpectedCloseDate = DateTime.Today.AddDays(21) },
        };
        }
    }
}
