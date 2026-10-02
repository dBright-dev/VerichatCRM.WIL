namespace VerichatCRM.API.DTOs.Requests
{
    public class OrderLineItemRequest
    {
        public string SKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}