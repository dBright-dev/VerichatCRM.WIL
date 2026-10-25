namespace VerichatCRM.Web.Models.Telehealth;

public class TelehealthAppointmentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string RequestedByUserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; }
}
