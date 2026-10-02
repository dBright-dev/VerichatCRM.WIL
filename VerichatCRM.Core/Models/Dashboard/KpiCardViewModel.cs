using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Dashboard
{
    public class KpiCardViewModel
    {
        public string Label { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string? DeltaLabel { get; set; }
        public string BadgeVariant { get; set; } = "accent";
    }
}
