namespace VerichatCRM.Web.Models.Client
{
    public class AssignmentHistoryItem
    {
        public string SalesRepId { get; set; } = "";
        public string SalesRepName { get; set; } = "";
        public DateTime AssignedDate { get; set; }
        public string? AssignedByName { get; set; }
    }
}