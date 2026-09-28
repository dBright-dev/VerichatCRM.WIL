namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateOrderRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public List<OrderLineItemRequest> LineItems { get; set; } = new();
    }
}
