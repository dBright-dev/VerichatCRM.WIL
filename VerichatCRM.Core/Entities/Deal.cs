using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Enums;

namespace VerichatCRM.Core.Entities
{
    public class Deal
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public int AssignedRepId { get; set; }
        public User AssignedRep { get; set; } = null!;
        public DealStage Stage { get; set; } = DealStage.InitialInquiry;
        public decimal Value { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
    }
}
