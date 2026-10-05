using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Minutes of exercise logged for one day of a week (DayIndex 0=Monday).</summary>
    public class WeeklyExerciseLog
    {
        [Key]
        public string LogId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public DateTime WeekStart { get; set; } // Monday of the week
        public int DayIndex { get; set; }
        public int Minutes { get; set; }
    }
}
