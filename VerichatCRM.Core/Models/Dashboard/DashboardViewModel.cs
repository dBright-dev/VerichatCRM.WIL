using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Dashboard
{
    public class DashboardViewModel
    {
        public string RoleLabel { get; set; } = string.Empty;

        public List<KpiCardViewModel> Kpis { get; set; } = new();

        public List<ChartPointViewModel> RevenueTrend { get; set; } = new();

        public List<ActivityItemViewModel> RecentActivity { get; set; } = new();
    }
}
