using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class PipelineController : Controller
    {
        private readonly IPipelineApiService _pipeline;

        public PipelineController(IPipelineApiService pipeline) => _pipeline = pipeline;

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Pipeline Board";
            ViewData["Subtitle"] = "Drag cards between stages to update deal progress.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";

            var model = await _pipeline.GetBoardAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MoveDeal([FromBody] MoveDealRequest request)
        {
            var ok = await _pipeline.MoveDealAsync(request.DealId, request.NewStage);
            return ok ? Ok() : BadRequest();
        }

        public class MoveDealRequest
        {
            public string DealId { get; set; } = "";
            public string NewStage { get; set; } = "";
        }
    }
}