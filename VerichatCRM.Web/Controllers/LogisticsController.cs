using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Logistics;

namespace VerichatCRM.Web.Controllers
{
    public class LogisticsController : Controller
    {
        private readonly ISupportApiService _support;

        public LogisticsController(ISupportApiService support) => _support = support;

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        // Logistics used to be sent to the support dashboard, so a dispatcher and a
        // field support engineer saw one screen with the same four tiles. This is
        // the dispatcher's own board, and it is built from what they act on: which
        // lane each delivery is in, what is past its ETA, and what is due service.
        [RequirePermission(Perm.LogisticsTracker)]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var deliveries = await _support.GetDeliveriesAsync();
            var assets = await _support.GetAssetsAsync();
            var now = DateTime.UtcNow;

            var open = deliveries
                .Where(d => d.Status != "Delivered" && d.Status != "Cancelled")
                .GroupBy(d => d.Status)
                .OrderByDescending(g => g.Count())
                .ToList();

            var model = new DispatchDashboardViewModel
            {
                Lanes = open.Select(g => g.Key).ToList(),
                ByLane = open.ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.EstimatedArrival).ToList()),
                Late = deliveries
                    .Where(d => d.Status != "Delivered" && d.Status != "Cancelled" && d.EstimatedArrival < now)
                    .OrderBy(d => d.EstimatedArrival)
                    .Take(8)
                    .ToList(),
                DeliveredThisWeek = deliveries.Count(d =>
                    d.ActualArrival >= now.AddDays(-7) && d.ActualArrival <= now),
                ServiceDue = assets
                    .Where(a => a.NextServiceDue <= now.AddDays(30) || a.Status == "Needs Service")
                    .OrderBy(a => a.NextServiceDue ?? DateTime.MaxValue)
                    .Take(10)
                    .ToList(),
                StatusCounts = deliveries.GroupBy(d => d.Status).ToDictionary(g => g.Key, g => g.Count())
            };

            ViewData["Title"] = "Dispatch Dashboard";
            ViewData["Subtitle"] = $"Lanes, late ETAs and service due — {DateTime.Now:dddd, d MMMM yyyy}";
            return View(model);
        }

        [RequirePermission(Perm.LogisticsTracker)]
        public async Task<IActionResult> Tracker()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Logistics Tracker";
            ViewData["Subtitle"] = "Order dispatch, delivery tracking & route management";
            var deliveries = await _support.GetDeliveriesAsync();
            return View(deliveries);
        }

        [RequirePermission(Perm.AssetTracking)]
        public async Task<IActionResult> Assets()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var assets = await _support.GetAssetsAsync();
            return View(assets);
        }

        [RequirePermission(Perm.AssetTracking)]
        public async Task<IActionResult> AssetDetail(string id)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var assets = await _support.GetAssetsAsync();
            var asset = assets.FirstOrDefault(a => a.AssetId == id || a.SerialNumber == id);
            if (asset == null) return NotFound();
            ViewData["Title"] = asset.Model;
            return View(asset);
        }
    }
}
