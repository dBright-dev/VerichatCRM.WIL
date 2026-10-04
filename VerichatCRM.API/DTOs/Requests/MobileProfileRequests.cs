using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    public class BookAppointmentRequest
    {
        [Required]
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Facility { get; set; } = string.Empty;
        public DateTime StartsAt { get; set; } = DateTime.UtcNow.AddDays(1);
    }

    public class UpdateProfileRequest
    {
        public string? DisplayName { get; set; }
        public string? Phone { get; set; }
        public string? Company { get; set; }
        public string? VatNumber { get; set; }
        public string? AddressName { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }

    public class UpdateSettingsRequest
    {
        public bool? PushNotifications { get; set; }
        public bool? BiometricLogin { get; set; }
        public bool? DarkMode { get; set; }
    }
}
