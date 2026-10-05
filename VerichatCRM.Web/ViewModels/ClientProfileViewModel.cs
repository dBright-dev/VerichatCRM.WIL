using VerichatCRM.Core.Models.Clients;

namespace VerichatCRM.Web.ViewModels;

public class ClientProfileViewModel
{
    public ClientViewModel Client { get; set; } = new();
    public List<ClientOrderRow> Orders { get; set; } = new();
    public List<ClientQuotationRow> Quotations { get; set; } = new();
}

public class ClientOrderRow
{
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<ClientOrderLineRow> LineItems { get; set; } = new();
}

public class ClientOrderLineRow
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}

public class ClientQuotationRow
{
    public string QuotationId { get; set; } = string.Empty;
    public string QuoteReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ValidUntil { get; set; }
    public decimal GrandTotal { get; set; }
    public List<ClientOrderLineRow> LineItems { get; set; } = new();
}
