namespace VerichatCRM.Web.Models.Client
{
    public class ClientDto
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string Sector { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string ContactEmail { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public string ContractStatus { get; set; } = "";
        public string? AssignedRepId { get; set; }
        public DateTime? LastContactDate { get; set; }
        public decimal TotalDealsValue { get; set; }
    }
}
