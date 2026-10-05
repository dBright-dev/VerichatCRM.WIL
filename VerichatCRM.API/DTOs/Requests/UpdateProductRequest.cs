namespace VerichatCRM.API.DTOs.Requests
{
    /// <summary>
    /// Partial update of a product. Every field is optional: omitted values are
    /// left untouched, so a sales representative can change only the price.
    /// </summary>
    public class UpdateProductRequest
    {
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public int? LowStockThreshold { get; set; }
        /// <summary>Send an empty string to clear the image.</summary>
        public string? ImageUrl { get; set; }
    }
}