namespace VerichatCRM.Web.Models.Deal
{
    public class DealDto
    {
        public string DealId { get; set; } = "";
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Stage { get; set; } = "";
        public decimal Revenue { get; set; }
        public string? AssignedRep { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public int Probability { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
