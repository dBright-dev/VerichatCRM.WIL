namespace VerichatCRM.Web.Models
{
    // Display rules for "last contact" and "next follow-up". Kept in one place so the pipeline board and the lead tracker always agree.
    public static class FollowUpHelper
    {
        // A deal with no contact for longer than this is flagged overdue.
        public const int OverdueAfterDays = 7;

        public static int? DaysSince(DateTime? date)
        {
            if (date is null) return null;
            var days = (int)Math.Floor((DateTime.UtcNow - date.Value).TotalDays);
            return Math.Max(days, 0);
        }

        public static string Describe(DateTime? lastContact)
        {
            var days = DaysSince(lastContact);
            return days switch
            {
                null => "No contact logged",
                0 => "Today",
                1 => "Yesterday",
                _ => $"{days} days ago"
            };
        }

        // True only when a contact date exists and is older than the rule.
        public static bool IsStale(DateTime? lastContact) =>
            DaysSince(lastContact) > OverdueAfterDays;

        // Next follow-up is today or earlier.
        public static bool IsFollowUpDue(DateTime? next) =>
            next is not null && next.Value.Date <= DateTime.UtcNow.Date;

        // Next follow-up was before today.
        public static bool IsFollowUpOverdue(DateTime? next) =>
            next is not null && next.Value.Date < DateTime.UtcNow.Date;
    }
}