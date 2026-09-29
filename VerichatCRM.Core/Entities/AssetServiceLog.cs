namespace VerichatCRM.Core.Entities
{
    public class AssetServiceLog
    {
        public string LogId { get; set; } = Guid.NewGuid().ToString();
        public string AssetId { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; } = DateTime.UtcNow;
        public string PerformedByUserId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Outcome { get; set; }
        public string? PartsReplaced { get; set; }
    }
}