using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Entities
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string Status { get; set; } = "Prospect";
        public DateTime? LastContactDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int AssignedRepId { get; set; }
        public User AssignedRep { get; set; } = null!;
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
