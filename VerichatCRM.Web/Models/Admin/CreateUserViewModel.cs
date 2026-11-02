using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Web.Models.Admin;

/// <summary>
/// The Add user form on ▷ Users &amp; Roles. Its shape is the sign-up form's, with
/// one difference that is the whole reason this screen exists: <see cref="Role"/>
/// is chosen by a signed-in System Administrator, so it may be any of the seven
/// roles, including the five the public form refuses to offer.
/// </summary>
public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(150)]
    [Display(Name = "Full name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(200)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
    [Display(Name = "Temporary password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public string Role { get; set; } = string.Empty;
}
