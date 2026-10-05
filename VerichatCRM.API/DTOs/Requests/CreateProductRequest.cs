namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateProductRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int InitialStockQty { get; set; }
        public int LowStockThreshold { get; set; } = 10;
        /// <summary>Optional absolute URL of the product image.</summary>
        public string? ImageUrl { get; set; }
    }
}
