using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

/// <summary>
/// A note from somebody who cannot get into the system. The login screen offers it
/// as the way out when the account is locked, forgotten or stuck in review, and it
/// has to reach a person - so every one of these is both listed for an
/// administrator inside the app and mailed to the address that owns signups.
///
/// It is not a support ticket. A ticket is a broken instrument belonging to a
/// client who is already in the system; this is somebody outside it who is trying
/// to get in, and they have no client row to hang a ticket on.
/// </summary>
public class LoginHelpRequest
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>The role the person believed they were signing up or in as, when they know.</summary>
    [MaxLength(50)]
    public string? ExpectedRole { get; set; }

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Where the note came from, so a spike of them from one page is visible.</summary>
    [MaxLength(40)]
    public string Channel { get; set; } = "Web";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required, MaxLength(20)]
    public string Status { get; set; } = LoginHelpStatus.Open;

    [MaxLength(450)]
    public string? ResolvedByUserId { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public static class LoginHelpStatus
{
    public const string Open = "Open";
    public const string Resolved = "Resolved";

    public static readonly string[] All = { Open, Resolved };
}
