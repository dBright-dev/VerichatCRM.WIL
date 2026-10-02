namespace VerichatCRM.Web.Models.Support
{
    public class AssetServiceLogDto
    {
        public string LogId { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Outcome { get; set; }
        public string? PartsReplaced { get; set; }
    }
}
