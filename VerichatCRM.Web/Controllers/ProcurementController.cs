using Microsoft.AspNetCore.Mvc;
using VerichatCRM.Web.Authorization;
using VerichatCRM.Web.Models.Procurement;
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

        [RequirePermission(Perm.ProcurementDashboard)]
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

        [RequirePermission(Perm.ProductCatalog)]
        public async Task<IActionResult> ProductCatalog(string? category, string? search)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");
            SetHeader("Product Catalog", "Browse and search the full medical supplies catalogue.");

            var allProducts = await _products.GetAllAsync();

            // Category chips come from what is actually in the database, so a
            // category only appears once an administrator has created a product in it.
            var vm = new ProductCatalogViewModel();
            foreach (var c in allProducts.Select(p => p.Category)
                         .Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct()
                         .OrderBy(c => c))
            {
                vm.Categories.Add(c);
            }

            var products = allProducts;
            if (!string.IsNullOrWhiteSpace(category) && category != "All")
                products = products.Where(p => p.Category == category).ToList();
            if (!string.IsNullOrWhiteSpace(search))
                products = products.Where(p =>
                    p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.SKU.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            vm.Products = products.Select(p => new ProductCard
            {
                SKU = p.SKU,
                ProductName = p.ProductName,
                Category = p.Category,
                Price = p.Price,
                StockQty = p.StockQty,
                LowStockThreshold = p.LowStockThreshold,
                ImageUrl = p.ImageUrl
            }).ToList();
            vm.ActiveCategory = category ?? "All";
            vm.SearchQuery = search ?? string.Empty;

            return View(vm);
        }

        [RequirePermission(Perm.ProductCreate)]
        [HttpGet]
        public IActionResult ProductCreate() => ProductForm(null);

        [RequirePermission(Perm.ProductCreate)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductCreate(ProductViewModel model)
        {
            if (!ModelState.IsValid) return ProductForm(model);

            var created = await _products.CreateAsync(model);
            if (created == null)
            {
                ModelState.AddModelError(string.Empty,
                    "The product could not be saved. Check the details and try again.");
                return ProductForm(model);
            }

            TempData["Success"] = $"Product \"{created.ProductName}\" was created and is now live.";
            return RedirectToAction(nameof(ProductCatalog));
        }

        [RequirePermission(Perm.ProductEdit)]
        [HttpGet]
        public async Task<IActionResult> ProductEdit(string sku)
        {
            var products = await _products.GetAllAsync();
            var product = products.FirstOrDefault(p => p.SKU == sku);
            if (product == null) return NotFound();

            return ProductForm(product);
        }

        [RequirePermission(Perm.ProductEdit)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProductEdit(string sku, ProductViewModel model)
        {
            // The SKU identifies the row and is never edited.
            model.SKU = sku;
            if (!ModelState.IsValid) return ProductForm(model);

            var updated = await _products.UpdateAsync(sku, model);
            if (updated == null)
            {
                ModelState.AddModelError(string.Empty,
                    "The product could not be updated. Check the details and try again.");
                return ProductForm(model);
            }

            TempData["Success"] = $"\"{updated.ProductName}\" was updated.";
            return RedirectToAction(nameof(ProductCatalog));
        }

        private IActionResult ProductForm(ProductViewModel? model)
        {
            SetHeader(
                model == null || string.IsNullOrEmpty(model.SKU) ? "New Product" : "Edit Product",
                "Products saved here are published to the catalogue and the mobile app.");

            ViewData["IsEdit"] = model != null && !string.IsNullOrEmpty(model.SKU);
            return View("ProductForm", model ?? new ProductViewModel());
        }

        [RequirePermission(Perm.WarehouseAlerts)]
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
