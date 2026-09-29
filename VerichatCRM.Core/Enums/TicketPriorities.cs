using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Enums
{
    public static class TicketPriorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
        public const string Critical = "Critical";

        public static readonly string[] All = { Low, Medium, High, Critical };

        /// <summary>SLA response window in hours per priority.</summary>
        public static int GetSlaHours(string priority) => priority switch
        {
            Critical => 4,
            High => 12,
            Medium => 24,
            _ => 48
        };
    }
}
