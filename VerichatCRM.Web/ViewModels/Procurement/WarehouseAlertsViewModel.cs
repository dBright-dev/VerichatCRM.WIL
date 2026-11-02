namespace VerichatCRM.Web.ViewModels.Procurement
{
    public class WarehouseAlertsViewModel
    {
        public List<StockAlertRow> Alerts { get; set; } = new();
        public int CriticalCount { get; set; }
        public string CriticalItemName { get; set; } = string.Empty;
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

        public bool CanReorder => ReorderQty > 0;

        /// <summary>
        /// The alert feed's own word — "Critical" or "Warning" — carried straight
        /// through rather than renamed. This screen used to translate Warning to
        /// "Low", which is a priority word, so the buyer's two stock screens showed
        /// the same shortage under two different names and the badge mapping had to
        /// give "Low" two meanings to cope.
        /// </summary>
        public string Severity { get; set; } = string.Empty;
    }
}
