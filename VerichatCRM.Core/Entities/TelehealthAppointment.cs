using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Telehealth appointment visible in the mobile app.</summary>
    public class TelehealthAppointment
    {
        [Key]
        public string AppointmentId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Facility { get; set; } = string.Empty;
        public DateTime StartsAt { get; set; }
        public string Status { get; set; } = "pending"; // confirmed | cancelled | pending | completed
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
