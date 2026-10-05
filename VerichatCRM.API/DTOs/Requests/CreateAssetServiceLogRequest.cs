namespace VerichatCRM.API.DTOs.Requests
{
    public class CreateAssetServiceLogRequest
    {
        public DateTime ServiceDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Outcome { get; set; }
        public string? PartsReplaced { get; set; }
    }
}
