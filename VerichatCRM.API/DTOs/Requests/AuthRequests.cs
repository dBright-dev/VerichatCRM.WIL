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