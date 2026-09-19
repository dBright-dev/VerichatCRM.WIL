using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.Core.Models.Pipeline
{
    internal class PipelineBoardViewModel
    {
        public Dictionary<string, List<DealViewModel>> DealsByStage { get; set; } = new();
        public decimal TotalPipelineValue { get; set; }
        public decimal WeightedForecast { get; set; }
        public int DealsThisPeriod { get; set; }

    }
}
