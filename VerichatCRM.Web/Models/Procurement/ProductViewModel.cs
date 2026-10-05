namespace VerichatCRM.Web.Models.Procurement
{
    public class ProductViewModel
    {
        public string SKU { get; set; } = "";
        public string ProductName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public int LowStockThreshold { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsLowStock => StockQty <= LowStockThreshold;
        public string StatusBadge => StockQty == 0 ? "Critical" : IsLowStock ? "Low" : "OK";
    }
}
