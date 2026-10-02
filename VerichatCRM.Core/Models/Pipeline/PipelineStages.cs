using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Pipeline
{
    public static class PipelineStages
    {
        public const string InitialInquiry = "Initial Inquiry";
        public const string SiteAssessment = "Site Assessment";
        public const string ProposalReview = "Proposal Review";
        public const string QuotationSent = "Quotation Sent";
        public const string ClosedWon = "Closed Won";
        public static readonly IReadOnlyList<string> All = new[]
        {
        InitialInquiry, SiteAssessment, ProposalReview, QuotationSent, ClosedWon
        };
    }
}
