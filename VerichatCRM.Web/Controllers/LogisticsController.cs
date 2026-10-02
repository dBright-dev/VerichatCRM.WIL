using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class LogisticsController : Controller
    {
        private readonly ISupportApiService _support;

        public LogisticsController(ISupportApiService support) => _support = support;

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        public async Task<IActionResult> Tracker()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            ViewData["Title"] = "Logistics Tracker";
            ViewData["Subtitle"] = "Order dispatch, delivery tracking & route management";
            var deliveries = await _support.GetDeliveriesAsync();
            return View(deliveries);
        }

        public async Task<IActionResult> Assets()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            var assets = await _support.GetAssetsAsync();
            return View(assets);
        }

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
