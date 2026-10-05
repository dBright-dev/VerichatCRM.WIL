namespace VerichatCRM.Web.ViewModels.Procurement
{

    public class ProductCatalogViewModel
    {
        public List<ProductCard> Products { get; set; } = new();
        /// <summary>Derived from the products in the database - never hard-coded.</summary>
        public List<string> Categories { get; set; } = new() { "All" };
        public string ActiveCategory { get; set; } = "All";
        public string SearchQuery { get; set; } = string.Empty;
    }

    public class ProductCard
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public int LowStockThreshold { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsLowStock => StockQty <= LowStockThreshold;
    }
}
