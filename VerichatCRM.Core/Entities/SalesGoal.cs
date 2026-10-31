using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public static class SalesGoalTypes
{
    public const string Monthly = "Monthly";
    public const string Yearly = "Yearly";

    public static readonly string[] All =
    {
        Monthly, Yearly
    };
}

public class SalesGoal
{
    [Key]
    public int GoalId { get; set; }

    public string UserId { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } = SalesGoalTypes.Monthly;

    public User? User { get; set; }
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
