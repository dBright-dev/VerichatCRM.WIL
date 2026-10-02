namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateClientRequest
    {
        [Required, MaxLength(200)] public string ClientName { get; set; } = string.Empty;
        [Required, MaxLength(100)] public string Sector { get; set; } = string.Empty;
        [MaxLength(150)] public string ContactPerson { get; set; } = string.Empty;
        [EmailAddress, MaxLength(200)] public string ContactEmail { get; set; } = string.Empty;
        [MaxLength(30)] public string ContactPhone { get; set; } = string.Empty;
        [MaxLength(500)] public string Address { get; set; } = string.Empty;
    }
}
