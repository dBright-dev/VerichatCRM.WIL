namespace VerichatCRM.API.DTOs.Responses
{
    public class ClientDto
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContractStatus { get; set; } = string.Empty;
        public string? AssignedRepId { get; set; }
        public DateTime? LastContactDate { get; set; }
        public decimal TotalDealsValue { get; set; }
    }
}
