using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Home-care device registered by a mobile user (scale, thermometer, BP cuff, ...).</summary>
    public class MobileDevice
    {
        [Key]
        public string DeviceId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "other"; // weightScale | thermometer | bloodPressure | glucoseMeter | heartRate | other
        public string Status { get; set; } = "connected"; // connected | disconnected
        /// <summary>BLE peripheral id (MAC / UUID) of the physical device, used to reconnect.</summary>
        public string? ExternalId { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
