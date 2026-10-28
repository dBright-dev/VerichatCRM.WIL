using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Core.Models.Dashboard;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Analytics;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class AnalyticsController : Controller
    {
        // Web defaults = camelCase; the default encoder also escapes < and > so the JSON is safe in a script tag.
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly IApiClient _api;
        private readonly IWebHostEnvironment _env;

        public AnalyticsController(IApiClient api, IWebHostEnvironment env)
        {
            _api = api;
            _env = env;
        }

        [Route("dashboard/analytics")]
        [RequirePermission(Perm.DashboardAnalytics)]
        public async Task<IActionResult> Index(bool demo = false)
        {
            var role = HttpContext.Session.GetString("UserRole") ?? "";
            var useDemo = demo && _env.IsDevelopment();

            var overview = await SafeGet<DashboardAnalyticsResponse>("/api/dashboard/analytics")
                           ?? new DashboardAnalyticsResponse();
            var charts = useDemo ? DemoCharts() : await SafeGet<AnalyticsDashboardDto>("/api/analytics/dashboard");

            ViewData["Title"] = "Analytics";
            ViewData["Subtitle"] = role == Roles.SalesRepresentative
                ? "Your sales performance, pipeline and sectors"
                : "Company-wide sales performance, pipeline and sectors";
            ViewData["CurrentRole"] = role;

            return View(new AnalyticsPageViewModel
            {
                Overview = overview,
                Charts = charts,
                ChartsJson = JsonSerializer.Serialize(charts, JsonOptions),
                IsDemo = useDemo
            });
        }

        private async Task<T?> SafeGet<T>(string url) where T : class
        {
            try { return await _api.GetAsync<T>(url); }
            catch { return null; }
        }

        // Development-only sample data so the charts can be checked before the API supplies them.
        private static AnalyticsDashboardDto DemoCharts() => new()
        {
            SalesVsTarget = new()
            {
                new() { Label = "May", Sales = 310000, Target = 400000 },
                new() { Label = "Jun", Sales = 420000, Target = 400000 },
                new() { Label = "Jul", Sales = 380000, Target = 450000 },
                new() { Label = "Aug", Sales = 465000, Target = 450000 },
                new() { Label = "Sep", Sales = 440000, Target = 500000 },
                new() { Label = "Oct", Sales = 320000, Target = 500000 }
            },
            Funnel = new() { Inquiries = 48, Quotes = 30, Won = 14 },
            SectorRevenue = new()
            {
                new() { Sector = "Mine", Revenue = 450000 },
                new() { Sector = "Hospital", Revenue = 300000 },
                new() { Sector = "Education", Revenue = 150000 },
                new() { Sector = "Clinic", Revenue = 100000 }
            }
        };
    }
}