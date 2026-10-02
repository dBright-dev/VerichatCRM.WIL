using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Clients
{
    public class ClientViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty; // e.g. Mining, Clinical, Manufacturing
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "Active"; // Active / Inactive / Prospect
        public string AssignedRep { get; set; } = string.Empty;
        public decimal TotalDealsValue { get; set; }
        public DateTime? LastContactDate { get; set; }

        public string StatusBadgeVariant =>
            Status switch
            {
                "Active" => "success",
                "Prospect" => "accent",
                "Inactive" => "warning",
                _ => "accent"
            };
    }
}
