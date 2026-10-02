using System.Collections.Generic;

namespace VerichatCRM.Web.Models.Support
{
    public class CreateTicketDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string? EquipmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string Category { get; set; } = "General";
        public List<string> PhotoUrls { get; set; } = new();
    }
}
