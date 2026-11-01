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
        private readonly IApiClient _api;

        public ProcurementController(
            IProductApiService products,
            IWarehouseAlertApiService alerts,
            IApiClient api)
        {
            _products = products;
            _alerts = alerts;
            _api = api;
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
            SetHeader("Stock Dashboard", $"What is on the shelf, what is short and what has been ordered — {DateTime.Now:dddd, d MMMM yyyy}");

            // This action used to return ~/Views/Dashboard/Index.cshtml — the same
            // workspace board every other role sees — while a real
            // Views/Procurement/Dashboard.cshtml sat next to it unreferenced, drawing
            // a revenue chart and an activity feed from two list properties nothing
            // ever filled. The board below reads the stock the manager owns.
            var products = await _products.GetAllAsync();
            var alerts = await _alerts.GetActiveAsync();
            var purchaseOrders = await _api.GetAsync<List<PurchaseOrderSummary>>("/api/purchaseorder")
                ?? new List<PurchaseOrderSummary>();

            var vm = new StockDashboardViewModel
            {
                ManagerName = HttpContext.Session.GetString("UserName") ?? "Procurement Manager",
                CatalogItems = products.Count,
                CategoryCount = products
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .Count(),
                // Priced at the catalogue's own last sale price: this is a scale
                // figure for the shelf, not a valuation, and the only cost data in
                // the system is what a purchase order paid.
                StockValue = products.Sum(p => p.Price * p.StockQty),
                LowStockItems = products.Count(p => p.IsLowStock),
                OutOfStockItems = products.Count(p => p.StockQty == 0),
                // Ordered but not yet on the shelf. "Received" is the only status
                // PurchaseOrderService ever writes besides the three here.
                OpenOrders = purchaseOrders.Count(po => po.Status is "Draft" or "Submitted" or "Approved")
            };

            vm.ReorderQueue = alerts
                .OrderBy(a => a.CurrentStock)
                .ThenBy(a => a.ProductName)
                .Take(5)
                .Select(a => new ReorderRow
                {
                    SKU = a.SKU,
                    ProductName = a.ProductName,
                    CurrentStock = a.CurrentStock,
                    Threshold = a.Threshold,
                    Severity = a.Severity
                })
                .ToList();

            // "Uncategorised" is a real bucket, not a filler: a product saved without
            // a category has to be counted somewhere or the chart quietly drops stock.
            var holdings = products
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Category) ? "Uncategorised" : p.Category.Trim())
                .OrderByDescending(g => g.Sum(p => p.StockQty))
                .Take(8)
                .ToList();
            vm.CategoryLabels = holdings.Select(g => g.Key).ToList();
            vm.CategoryUnits = holdings.Select(g => g.Sum(p => p.StockQty)).ToList();

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
            var products = await _products.GetAllAsync();
            var purchaseOrders = await _api.GetAsync<List<PurchaseOrderSummary>>("/api/purchaseorder")
                ?? new List<PurchaseOrderSummary>();

            // There is no supplier master data in this system, so the only real
            // answer to "who supplies this?" is whoever the last purchase order for
            // that SKU went to. Anything else would be invented, and an invented
            // supplier ends up printed on a purchase order.
            string LastSupplierFor(string sku) => purchaseOrders
                .Where(po => po.LineItems.Any(li => string.Equals(li.SKU, sku, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(po => po.CreatedAt)
                .Select(po => po.SupplierName)
                .FirstOrDefault(string.Empty);

            var vm = new WarehouseAlertsViewModel
            {
                Alerts = alerts.Select(a => new StockAlertRow
                {
                    SKU = a.SKU,
                    ProductName = a.ProductName,
                    CurrentStock = a.CurrentStock,
                    MinStock = a.Threshold,
                    ReorderQty = Math.Max(a.Threshold * 2 - a.CurrentStock, 0),
                    Supplier = LastSupplierFor(a.SKU),
                    SuggestedUnitCost = products
                        .FirstOrDefault(p => string.Equals(p.SKU, a.SKU, StringComparison.OrdinalIgnoreCase))?.Price ?? 0m,
                    Severity = a.Severity
                }).ToList()
            };
            vm.CriticalCount = vm.Alerts.Count(a => a.Severity == "Critical");
            vm.CriticalItemName = vm.Alerts.FirstOrDefault(a => a.Severity == "Critical")?.ProductName ?? string.Empty;
            return View(vm);
        }

        /// <summary>
        /// Creates the purchase order for one low-stock row. This is a form POST
        /// rather than page script calling the API: the JWT lives in the session,
        /// not in the browser, so only this server can authenticate to the API.
        /// </summary>
        [HttpPost, ValidateAntiForgeryToken]
        [RequirePermission(Perm.WarehouseAlerts)]
        public async Task<IActionResult> Reorder(string? sku, string? supplier, decimal unitCost)
        {
            if (!IsAuthenticated) return RedirectToAction("Login", "Account");

            var trimmedSupplier = (supplier ?? string.Empty).Trim();
            var problem = trimmedSupplier.Length switch
            {
                0 => "Choose the supplier this order goes to.",
                > 200 => "The supplier name must be 200 characters or fewer.",
                _ when unitCost <= 0m => "Enter the purchase cost per unit; it cannot be zero.",
                _ => null
            };

            if (problem != null || string.IsNullOrWhiteSpace(sku))
            {
                TempData["ReorderMessage"] = problem ?? "Pick a product to reorder.";
                return RedirectToAction(nameof(WarehouseAlerts));
            }

            sku = sku.Trim();
            var alert = (await _alerts.GetActiveAsync())
                .FirstOrDefault(a => string.Equals(a.SKU, sku, StringComparison.OrdinalIgnoreCase));
            var product = (await _products.GetAllAsync())
                .FirstOrDefault(p => string.Equals(p.SKU, sku, StringComparison.OrdinalIgnoreCase));

            if (product == null)
            {
                TempData["ReorderMessage"] = $"{sku} is no longer in the catalogue, so it cannot be reordered.";
                return RedirectToAction(nameof(WarehouseAlerts));
            }

            // The quantity is recomputed here instead of trusted from the form: it
            // is the replenishment rule the page shows, and a tampered value would
            // order stock nobody asked for.
            var quantity = alert == null
                ? product.LowStockThreshold
                : Math.Max(alert.Threshold * 2 - alert.CurrentStock, 1);

            var created = await _api.PostAsync<CreatePurchaseOrderBody, PurchaseOrderSummary>(
                "/api/purchaseorder",
                new CreatePurchaseOrderBody(trimmedSupplier,
                    new[] { new PurchaseOrderLineBody(product.SKU, quantity, unitCost) }));

            // The confirmation quotes the supplier the API stored, not the string
            // that arrived: markup is stripped on the way in, so echoing the input
            // would name a supplier that no order was raised against.
            TempData["ReorderMessage"] = created == null
                ? _api.LastError ?? $"The reorder for {product.ProductName} could not be created."
                : $"Purchase order raised: {quantity} × {product.ProductName} from {created.SupplierName}.";

            return RedirectToAction(nameof(WarehouseAlerts));
        }

        private sealed record CreatePurchaseOrderBody(string SupplierName, IEnumerable<PurchaseOrderLineBody> LineItems);

        private sealed record PurchaseOrderLineBody(string SKU, int Quantity, decimal UnitCost);

        private sealed record PurchaseOrderSummary(
            string PurchaseOrderId,
            string SupplierName,
            List<PurchaseOrderLineSummary> LineItems,
            decimal TotalCost,
            string Status,
            DateTime CreatedAt);

        private sealed record PurchaseOrderLineSummary(string SKU, int Quantity, decimal UnitCost);
    }
}
