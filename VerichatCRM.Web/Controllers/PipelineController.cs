using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    [RequirePermission(Perm.PipelineBoard)]
    public class PipelineController : Controller
    {
        private readonly IPipelineApiService _pipeline;
        private readonly IApiClient _api;

        public PipelineController(IPipelineApiService pipeline, IApiClient api) { _pipeline = pipeline; _api = api; }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("JwtToken") == null)
                return RedirectToAction("Login", "Account");

            ViewData["Title"] = "Pipeline Board";
            ViewData["Subtitle"] = "Drag cards between stages to update deal progress.";
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Sales Representative";
            ViewBag.ApiBaseUrl = HttpContext.RequestServices.GetRequiredService<IConfiguration>()["ApiBaseUrl"]?.TrimEnd('/');
            ViewBag.ApiToken = System.Text.Json.JsonSerializer.Serialize(HttpContext.Session.GetString("JwtToken") ?? string.Empty);

            var model = await _pipeline.GetBoardAsync();
            var quotations = await _api.GetAsync<List<VerichatCRM.Web.Models.Quotation.QuotationDto>>("/api/quotation") ?? new();
            ViewBag.MobileQuoteRequests = quotations.Where(q => q.Terms.StartsWith("Mobile quote request", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(q => q.DateGenerated).ToList();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MoveDeal([FromBody] MoveDealRequest request)
        {
            if (HttpContext.Session.GetString("JwtToken") == null) return Unauthorized();
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
