using System.ComponentModel.DataAnnotations;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.API.DTOs.Requests
{
    public class ClientRegisterRequest
    {
        [Required, MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? CompanyName { get; set; }
    }
}
