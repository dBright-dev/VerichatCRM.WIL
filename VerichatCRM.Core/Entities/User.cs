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

    // An account that a person created for themselves is not evidence that they
    // are who they say they are. The sign-up form is public, so anyone who wants
    // a Sales Representative or Procurement Manager login - with the client
    // directories, quotations and reorder controls that role carries - can simply
    // ask for one. Approval is the answer to that: a self-registered account stops
    // at Pending and stays there until an employee has read it and approved it.
    //
    // The state is a word rather than a bit because declining an application has to
    // say something, and because "not yet reviewed" and "reviewed and refused" are
    // different things to explain to the person waiting.
    [Required, MaxLength(20)]
    public string ApprovalStatus { get; set; } = UserApproval.Pending;

    [MaxLength(450)]
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    [MaxLength(1000)]
    public string? RejectionReason { get; set; }

    // An address nobody confirmed is an address nobody owns, and the password reset
    // and approval notices both go there. Verification is recorded but not enforced
    // as a wall: an unverified account that an employee has approved can still work,
    // because the approval is the stronger claim on this system.
    public bool EmailVerified { get; set; }

    [MaxLength(256)]
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }

    [MaxLength(256)]
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetExpiry { get; set; }
    /// <summary>Expo push notification token for mobile app.</summary>
    [MaxLength(512)]
    public string? DeviceToken { get; set; }
}
