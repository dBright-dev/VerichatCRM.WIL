using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Single health telemetry reading logged from the mobile app / device sync.</summary>
    public class VitalReading
    {
        [Key]
        public string ReadingId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // bloodPressure | heartRate | bloodGlucose | temperature | bodyWeight
        public string Value { get; set; } = string.Empty; // e.g. "118/76"
        public string Unit { get; set; } = string.Empty;  // e.g. "mmHg"
        public string StatusLabel { get; set; } = "Normal";
        public bool InRange { get; set; } = true;
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
