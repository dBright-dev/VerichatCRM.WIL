using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public static class AppointmentStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Declined = "Declined";
    public static readonly string[] All = [Pending, Approved, Declined];
}

/// <summary>A client request for a B2B consultation or field service visit.</summary>
public class Appointment
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string RequestedByUserId { get; set; } = string.Empty;
    [Required, MaxLength(160)]
    public string Topic { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    [Required, MaxLength(24)]
    public string Status { get; set; } = AppointmentStatuses.Pending;
    [MaxLength(4000)]
    public string? Notes { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
