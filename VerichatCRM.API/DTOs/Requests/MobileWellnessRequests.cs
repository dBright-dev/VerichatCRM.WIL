using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Requests
{
    public class LogVitalRequest
    {
        [Required]
        public string Type { get; set; } = string.Empty; // bloodPressure | heartRate | bloodGlucose | temperature | bodyWeight

        [Required]
        public string Value { get; set; } = string.Empty; // e.g. "118/76"

        public string Unit { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = "Normal";
        public bool InRange { get; set; } = true;
        public DateTime? RecordedAt { get; set; }
    }

    public class RegisterDeviceRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "other";

        /// <summary>Optional BLE peripheral id so the app can reconnect to the same physical device.</summary>
        public string? ExternalId { get; set; }
    }

    /// <summary>A single reading pulled from a connected device during sync.</summary>
    public class DeviceSyncReadingDto
    {
        [Required]
        public string Type { get; set; } = string.Empty; // bloodPressure | heartRate | bloodGlucose | temperature | bodyWeight

        [Required]
        public string Value { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = "Normal";
        public bool InRange { get; set; } = true;
        public DateTime? RecordedAt { get; set; }
    }

    /// <summary>Body of POST /api/mobile/devices/{id}/sync — readings are optional.</summary>
    public class SyncDeviceRequest
    {
        public List<DeviceSyncReadingDto>? Readings { get; set; }
    }

    public class UpdateGoalRequest
    {
        public string? Type { get; set; } // steps | water | sleep | exercise
        public double? Current { get; set; }
        public double? Target { get; set; }
        public string? Name { get; set; }
        public string? Unit { get; set; }
    }

    public class LogExerciseRequest
    {
        [Range(0, 6)]
        public int DayIndex { get; set; } // 0=Monday
        public int Minutes { get; set; }
    }
}
