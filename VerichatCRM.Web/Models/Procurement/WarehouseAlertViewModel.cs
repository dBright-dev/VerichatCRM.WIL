namespace VerichatCRM.Web.Models.Procurement
{
    public class WarehouseAlertViewModel
    {
        public string AlertId { get; set; } = "";
        public string SKU { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }
        public string Severity { get; set; } = "Warning";
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }
}
