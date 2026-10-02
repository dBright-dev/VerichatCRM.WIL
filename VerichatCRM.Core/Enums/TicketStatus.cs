using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Enums
{
    public static class TicketStatuses
    {
        public const string Received = "Received";
        public const string UnderReview = "Under Review";
        public const string ActionTaken = "Action Taken";
        public const string Resolved = "Resolved";
        public const string Escalated = "Escalated";
        public const string Closed = "Closed";

        public static readonly string[] All =
        {
        Received, UnderReview, ActionTaken, Resolved, Escalated, Closed
    };
    }
}
