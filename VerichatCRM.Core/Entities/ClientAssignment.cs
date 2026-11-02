using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public class ClientAssignment
{
    [Key]
    public int ClientAssignmentId { get; set; }

    public string ClientId { get; set; } = string.Empty;
    public string SalesRepId { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    public Client? Client { get; set; }
    public User? SalesRep { get; set; }
}
