using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Web.Models.Auth
{
    /// <summary>The one field of the "I cannot get in" form.</summary>
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Enter the email address your account was created with.")]
        [EmailAddress(ErrorMessage = "That is not an email address.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// The new password plus the token that authorises the change. The token arrives
    /// in the emailed link, is shown to nobody, and is posted back unchanged - so it
    /// is a hidden field rather than something the user is asked to understand.
    /// </summary>
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type a new password.")]
        [MinLength(8, ErrorMessage = "Use at least 8 characters.")]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type it once more to be sure.")]
        [Compare(nameof(NewPassword), ErrorMessage = "The two passwords are not the same.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// The single screen every recovery flow ends on: the reset was asked for, the
    /// password was changed, the address was confirmed, or one of those failed.
    /// </summary>
    public class RecoveryNoticeViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// True when the screen is reporting a problem rather than something that
        /// worked. It picks the banner's tone and the next button, so a person who
        /// clicked an expired link is not shown the same page as one who succeeded.
        /// </summary>
        public bool IsProblem { get; set; }

        /// <summary>The address the flow is about, offered back so it can be edited.</summary>
        public string? Email { get; set; }

        public static RecoveryNoticeViewModel Worked(string title, string body, string? email = null) =>
            new() { Title = title, Body = body, Email = email };

        public static RecoveryNoticeViewModel Problem(string title, string body, string? email = null) =>
            new() { Title = title, Body = body, IsProblem = true, Email = email };
    }
}
