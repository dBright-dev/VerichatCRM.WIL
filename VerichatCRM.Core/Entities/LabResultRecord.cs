using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>Lab result pushed from the LIMS to a mobile user.</summary>
    public class LabResultRecord
    {
        [Key]
        public string LabResultId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string Laboratory { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "processing"; // ready | processing
        public int? FileSizeKb { get; set; }
        public bool IsNew { get; set; } = true;
        public string? PdfUrl { get; set; }
    }
}
