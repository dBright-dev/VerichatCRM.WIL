namespace VerichatCRM.Web.ViewModels.Procurement
{
    public class LogisticsTrackerViewModel
    {
        public List<ShipmentCard> Shipments { get; set; } = new();
        public List<string> Statuses { get; set; } = new() { "All", "Processing", "Dispatched", "In Transit", "Delivered" };
        public string ActiveStatus { get; set; } = "All";
    }

    public class ShipmentCard
    {
        public string OrderRef { get; set; } = string.Empty;
        public string Client { get; set; } = string.Empty;
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public string Status { get; set; } = "Processing";
        public DateTime Eta { get; set; }
        public decimal WeightKg { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
    }
}
