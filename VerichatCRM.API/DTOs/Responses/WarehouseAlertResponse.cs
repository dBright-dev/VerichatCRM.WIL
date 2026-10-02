namespace VerichatCRM.API.DTOs.Responses
{
    public class WarehouseAlertResponse
    {
        public string AlertId { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string Severity { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
