namespace VerichatCRM.Web.Models.Telehealth;

public class TelehealthAppointmentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }
    public string RequestedByUserId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? AssignedToUserId { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TelehealthAssigneeViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
