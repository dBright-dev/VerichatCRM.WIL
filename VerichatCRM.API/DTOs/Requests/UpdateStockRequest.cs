namespace VerichatCRM.API.DTOs.Requests
{
    public class UpdateStockRequest
    {
        public int NewQuantity { get; set; }
        public string Reason { get; set; } = string.Empty; // Restock, Adjustment, Damage
    }
}
