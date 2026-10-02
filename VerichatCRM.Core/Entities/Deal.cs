using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public class Deal
{
    [Key]
    public string DealId { get; set; } = Guid.NewGuid().ToString();
    public string? InquiryId { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string Stage { get; set; } = "Initial Inquiry";
    public decimal Revenue { get; set; }
    public string? OwnerUserId { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public int Probability { get; set; } = 25;              // 0-100
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Client? Client { get; set; }
    public List<Quotation> Quotations { get; set; } = new();
}
