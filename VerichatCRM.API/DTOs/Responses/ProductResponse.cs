namespace VerichatCRM.API.DTOs.Responses
{
    public class ProductResponse
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQty { get; set; }
        public bool IsLowStock { get; set; }
    }
}
