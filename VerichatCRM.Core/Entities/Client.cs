using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public class Client
{
    [Key]
    public string ClientId { get; set; } = Guid.NewGuid().ToString();
    public string ClientName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public string ContactPerson { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public string ContractStatus { get; set; } = "Active";  // Active | Prospect | Inactive
    public string? AssignedRepId { get; set; }              // FK → Users.UserId
    public DateTime? LastContactDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public List<KeyPersonnel> KeyPersonnel { get; set; } = new();
    public List<Deal> Deals { get; set; } = new();
    public List<Inquiry> Inquiries { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
    public List<Asset> Assets { get; set; } = new();
}

public class KeyPersonnel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}