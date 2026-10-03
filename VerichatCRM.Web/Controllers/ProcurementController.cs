using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Controllers
{
    public class ProcurementController : Controller
    {
        private readonly IProductApiService _products;
        private readonly IWarehouseAlertApiService _alerts;

        public ProcurementController(
            IProductApiService products,
            IWarehouseAlertApiService alerts)
        {
            _products = products;
            _alerts = alerts;
        }

        private void SetHeader(string title, string subtitle)
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? "Procurement Manager";
        }

        public async Task<IActionResult> Dashboard()
        {
            SetHeader("Procurement Dashboard", "Reorder consumables, track deliveries, monitor stock.");
            return View();
        }

        public async Task<IActionResult> ProductCatalog()
        {
            SetHeader("Product Catalog", "Browse and search the full medical supplies catalogue.");
            var products = await _products.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> WarehouseAlerts()
        {
            SetHeader("Warehouse Alerts", "Low-stock safeguard and reorder management.");
            var alerts = await _alerts.GetActiveAsync();
            return View(alerts);
        }
    }
}