namespace VerichatCRM.Web.Models.Goals
{
    public class GoalProgressViewModel
    {
        public decimal Target { get; set; }
        public decimal Achieved { get; set; }
        public decimal Percentage { get; set; }

        public bool HasGoal => Target > 0;
    }
}
