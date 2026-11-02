namespace VerichatCRM.Web.ViewModels.Procurement
{
    // The procurement manager's own home: what is on the shelf, what is short and
    // what has been ordered but not arrived. It reads the catalogue and the alert
    // feed, not the sales KPIs — /dashboard is where the cross-role numbers live.
    public class StockDashboardViewModel
    {
        public string ManagerName { get; set; } = "Procurement Manager";
        public int CatalogItems { get; set; }
        public int CategoryCount { get; set; }
        public decimal StockValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int OpenOrders { get; set; }

        // The worst five, so the board ends with something to act on rather than
        // only counts. Each row links into Warehouse Alerts, where the order is raised.
        public List<ReorderRow> ReorderQueue { get; set; } = new();

        // Labels and unit counts, in the order the bar chart should draw them.
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryUnits { get; set; } = new();
    }

    public class ReorderRow
    {
        public string SKU { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CurrentStock { get; set; }
        public int Threshold { get; set; }

        // Warning or Critical — the two words the alert feed itself produces, kept
        // rather than renamed to "Low" so the chip and the alerts screen agree.
        public string Severity { get; set; } = "Warning";
    }
}
