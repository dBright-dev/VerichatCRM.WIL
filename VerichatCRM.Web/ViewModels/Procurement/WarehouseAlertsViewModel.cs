namespace VerichatCRM.Web.ViewModels.Procurement
{
    public class WarehouseAlertsViewModel
    {
        public List<StockAlertRow> Alerts { get; set; } = new();
        public int CriticalCount { get; set; }
        public int LowCount { get; set; }
        public string CriticalItemName { get; set; } = string.Empty;
        public List<StockLevelPoint> StockLevels { get; set; } = new();
    }

    public class StockAlertRow
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int MinStock { get; set; }
        public int ReorderQty { get; set; }
        public string Supplier { get; set; } = string.Empty;

        /// <summary>
        /// Starting value for the purchase-order line cost. The catalogue holds a
        /// selling price and no purchase cost, so the buyer confirms or corrects
        /// this rather than the system implying a margin.
        /// </summary>
        public decimal SuggestedUnitCost { get; set; }

        public bool CanReorder => ReorderQty > 0 && Status != "OK";
        public string Status { get; set; } = "OK"; // OK, Low, Critical
    }

    public class StockLevelPoint
    {
        public string Month { get; set; } = string.Empty;
        public int Desktop { get; set; }
        public int Mobile { get; set; }
    }
}
