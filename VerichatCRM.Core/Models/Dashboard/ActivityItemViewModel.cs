using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Dashboard
{
    public class ActivityItemViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Timestamp { get; set; } = string.Empty;

        public string BadgeVariant { get; set; } = "accent";
    }
}
