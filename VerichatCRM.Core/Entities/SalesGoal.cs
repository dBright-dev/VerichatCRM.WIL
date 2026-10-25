using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public class SalesGoal
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string UserId { get; set; } = string.Empty;
    [Required, MaxLength(160)]
    public string Title { get; set; } = string.Empty;
    public decimal Target { get; set; }
    public decimal Achieved { get; set; }
    [Required, MaxLength(24)]
    public string Period { get; set; } = "monthly";
    [MaxLength(80)]
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
