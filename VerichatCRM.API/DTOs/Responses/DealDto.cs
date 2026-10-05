namespace VerichatCRM.API.DTOs.Responses
{
    public class DealDto
    {
        public string DealId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Stage { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public string? OwnerUserId { get; set; }
        public string? AssignedRep { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public int Probability { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
