using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Daily wellness goal (steps, water, sleep, exercise) for a mobile user.</summary>
    public class WellnessGoal
    {
        [Key]
        public string GoalId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // steps | water | sleep | exercise
        public string Name { get; set; } = string.Empty;
        public double Current { get; set; }
        public double Target { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
