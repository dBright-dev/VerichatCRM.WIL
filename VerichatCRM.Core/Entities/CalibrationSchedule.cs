using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public static class CalibrationScheduleStatuses
{
    public const string Pending = "Pending";
    public const string Completed = "Completed";

    public static readonly string[] All =
    {
        Pending, Completed
    };
}

public class CalibrationSchedule
{
    [Key]
    public int ScheduleId { get; set; }

    public string ClientId { get; set; } = string.Empty;
    public string EquipmentId { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = CalibrationScheduleStatuses.Pending;
    public string AssignedTechId { get; set; } = string.Empty;

    public Client? Client { get; set; }
    public Asset? Equipment { get; set; }
    public User? AssignedTech { get; set; }
}
