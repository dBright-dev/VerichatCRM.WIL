using System.Text.Json.Serialization;

namespace VerichatCRM.API.DTOs.Responses
{
    public class MobileProfileResponse
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("orderCount")]
        public int OrderCount { get; set; }

        [JsonPropertyName("memberSince")]
        public DateTime MemberSince { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("vatNumber")]
        public string? VatNumber { get; set; }

        [JsonPropertyName("defaultAddress")]
        public MobileAddressResponse? DefaultAddress { get; set; }
    }

    public class MobileAddressResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("street")]
        public string Street { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;
    }

    public class MobileSettingsResponse
    {
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("preferences")]
        public MobilePreferencesResponse Preferences { get; set; } = new();
    }

    public class MobilePreferencesResponse
    {
        [JsonPropertyName("pushNotifications")]
        public bool PushNotifications { get; set; } = true;

        [JsonPropertyName("biometricLogin")]
        public bool BiometricLogin { get; set; }

        [JsonPropertyName("darkMode")]
        public bool DarkMode { get; set; }
    }
}
