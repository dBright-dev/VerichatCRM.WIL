using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    /// <summary>E-prescription issued to a mobile user (QR payload scanned at pharmacy).</summary>
    public class MobilePrescription
    {
        [Key]
        public string PrescriptionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Medication { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public string Prescriber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "active"; // active | dispensed | expired
        public string QrPayload { get; set; } = string.Empty;
    }
}
