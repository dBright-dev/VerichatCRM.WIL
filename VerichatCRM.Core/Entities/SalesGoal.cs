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
}
