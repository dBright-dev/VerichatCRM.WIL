using System;

namespace VerichatCRM.Core.Models.Pipeline
{
    public class DealViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Stage { get; set; } = PipelineStages.InitialInquiry;
        public decimal Value { get; set; }
        public string AssignedRep { get; set; } = string.Empty;
        public DateTime? ExpectedCloseDate { get; set; }

        // Last time the linked client was contacted. Null until the API supplies it.
        public DateTime? LastContactDate { get; set; }
    }
}