using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities;

/// <summary>
/// CRM application account. Authentication identities and CRM staff/client
/// permissions use this record; password hashes and reset secrets are never
/// exposed to API callers.
/// </summary>
public class User
{
    [Key]
    public string UserId { get; set; } = Guid.NewGuid().ToString();

    [Required, MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Role { get; set; } = UserRoles.ClientUser;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    [MaxLength(256)]
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    /// <summary>Expo push notification token for mobile app.</summary>
    [MaxLength(512)]
    public string? DeviceToken { get; set; }
}
