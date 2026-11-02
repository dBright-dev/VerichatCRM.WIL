using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

public class ClientTimelineNote
{
    [Key]
    public string NoteId { get; set; } = Guid.NewGuid().ToString();
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required, MaxLength(4000)]
    public string Text { get; set; } = string.Empty;
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
