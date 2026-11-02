using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VerichatCRM.API.DTOs.Requests
{
    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// "I cannot get in." The one form in this file that a signed-out, and possibly
    /// never-registered, person fills in - so it asks for a name and an address to
    /// reply to rather than a credential to check.
    /// <para>
    /// <see cref="ExpectedRole"/> is free text and is never read as an instruction:
    /// it says what the person believed they were signing up as, which is the first
    /// thing an administrator needs in order to tell "I was given the wrong role" from
    /// "my account does not behave". Changing a role is still the users screen's job,
    /// from an authenticated administrator's session, not here.
    /// </para>
    /// </summary>
    public class AskForHelpRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ExpectedRole { get; set; }

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Which app the note was typed into. The mobile client sends "Mobile" so a
        /// queue can tell a broken build of the phone app from a broken sign-in page;
        /// it is the one field here the caller labels itself, and the only thing
        /// mislabelling it can cost is a wrong word in a column.
        /// </summary>
        [MaxLength(40)]
        public string? Channel { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required] public string Token { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// The token from an email-confirmation link, posted by the page the link opens.
    /// The link itself has to carry the token in its query string - it is the only
    /// thing the recipient has - so this POST is where the token stops being repeated:
    /// it is not written to the API's request logs, and no other page can refer to it.
    /// </summary>
    public class ConfirmEmailRequest
    {
        [Required] public string Token { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required] public string CurrentPassword { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    /// <summary>Administrator-created account. The role is validated against
    /// <see cref="UserRoles.All"/> in the service, never trusted verbatim.</summary>
    public class AdminCreateUserRequest
    {
        [Required, MaxLength(120)] public string DisplayName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        [Required, MaxLength(120)] public string DisplayName { get; set; } = string.Empty;
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Role { get; set; } = string.Empty;
    }
}