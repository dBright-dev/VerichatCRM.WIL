using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>
    /// A single health metric reading captured from a paired IoT / health device
    /// (e.g. Bluetooth scale, blood pressure cuff) and posted from the mobile app.
    /// </summary>
    public class HealthMetric
    {
        [Key]
        public string MetricId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Client the reading belongs to (Client.Id == mobile UserId).</summary>
        [Required]
        public string ClientId { get; set; } = string.Empty;

        /// <summary>Source device (MobileDevice.DeviceId) that produced the reading.</summary>
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>bloodPressure | heartRate | bloodGlucose | temperature | bodyWeight</summary>
        [Required]
        public string MetricType { get; set; } = string.Empty;

        [Required]
        public string Value { get; set; } = string.Empty; // e.g. "75.5" or "118/76"

        public string Unit { get; set; } = string.Empty; // e.g. "kg", "bpm", "mmHg"

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
