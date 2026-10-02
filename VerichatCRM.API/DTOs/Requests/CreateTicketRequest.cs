using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateTicketRequest
    {
        [Required] public string ClientId { get; set; } = string.Empty;
        public string? EquipmentId { get; set; }

        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [Required, MaxLength(4000)] public string Description { get; set; } = string.Empty;

        [Required] public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "General";
        public List<string> PhotoUrls { get; set; } = new();
    }
}
