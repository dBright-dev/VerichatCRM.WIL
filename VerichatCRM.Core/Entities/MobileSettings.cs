using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Per-user app preferences for the mobile app.</summary>
    public class MobileSettings
    {
        [Key]
        public string UserId { get; set; } = string.Empty;
        public bool PushNotifications { get; set; } = true;
        public bool BiometricLogin { get; set; }
        public bool DarkMode { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
