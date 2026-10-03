using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Services.Interfaces;
using VerichatCRM.Web.ViewModels.Procurement;

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

        private bool IsAuthenticated => HttpContext.Session.GetString("JwtToken") != null;

        private bool RequireRole(params string[] roles)
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role != null && roles.Contains(role);
        }

        private void SetHeader(string title, string subtitle, string fallbackRole = "Procurement Manager")
        {
            ViewData["Title"] = title;
            ViewData["Subtitle"] = subtitle;
            ViewData["CurrentRole"] = HttpContext.Session.GetString("UserRole") ?? fallbackRole;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            SetHeader("Procurement Dashboard", "Reorder consumables, track deliveries, monitor stock.");

            var products = await _products.GetAllAsync();
            var alerts = await _alerts.GetActiveAsync();
            var lowStock = products.Count(p => p.IsLowStock);

            var vm = new ProcurementDashboardViewModel
            {
                ManagerName = HttpContext.Session.GetString("UserName") ?? "Procurement Manager",
                CurrentDate = DateTime.Now,
                ActiveOrders = 0,
                PendingQuotes = 0,
                CatalogItems = products.Count,
                LowStockItems = alerts.Count + lowStock,
                RevenueVsTarget = new List<RevenuePoint>(),
                RecentActivity = alerts.Take(5).Select(a => new ActivityItem
                {
                    Title = $"Low stock: {a.ProductName} ({a.CurrentStock} left)",
                    Subtitle = a.Severity,
                    Timestamp = a.CreatedAt.ToString("HH:mm"),
                    Severity = a.Severity == "Critical" ? "critical" : "warning"
                }).ToList()
            };
            return View(vm);
        }

        public async Task<IActionResult> ProductCatalog(string? category, string? search)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            SetHeader("Product Catalog", "Browse and search the full medical supplies catalogue.");

            var products = await _products.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                products = products.Where(p => p.Category == category).ToList();
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p =>
                    p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            var vm = new ProductCatalogViewModel
            {
                Products = products.Select(p => new ProductCard
                {
                    SKU = p.SKU,
                    ProductName = p.ProductName,
                    Category = p.Category,
                    Price = p.Price,
                    StockQty = p.StockQty,
                    LowStockThreshold = p.LowStockThreshold
                }).ToList(),
                ActiveCategory = category ?? "All",
                SearchQuery = search ?? string.Empty
            };
            // Keep the fixed category chips plus any categories from data.
            foreach (var c in products.Select(p => p.Category).Distinct())
                if (!vm.Categories.Contains(c)) vm.Categories.Add(c);
            return View(vm);
        }

        public async Task<IActionResult> WarehouseAlerts()
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            SetHeader("Warehouse Alerts", "Low-stock safeguard and reorder management.");

            var alerts = await _alerts.GetActiveAsync();
            var vm = new WarehouseAlertsViewModel
            {
                Alerts = alerts.Select(a => new StockAlertRow
                {
                    SKU = a.SKU,
                    ProductName = a.ProductName,
                    CurrentStock = a.CurrentStock,
                    MinStock = a.Threshold,
                    ReorderQty = Math.Max(a.Threshold * 2 - a.CurrentStock, 0),
                    Supplier = string.Empty,
                    Status = a.Severity == "Critical" ? "Critical" : "Low"
                }).ToList()
            };
            vm.CriticalCount = vm.Alerts.Count(a => a.Status == "Critical");
            vm.LowCount = vm.Alerts.Count(a => a.Status == "Low");
            vm.CriticalItemName = vm.Alerts.FirstOrDefault(a => a.Status == "Critical")?.ProductName ?? string.Empty;
            return View(vm);
        }
    }
}
