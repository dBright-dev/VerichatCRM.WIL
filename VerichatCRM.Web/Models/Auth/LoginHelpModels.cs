using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Web.Models.Auth
{
    /// <summary>
    /// The form behind "I cannot get in". It is anonymous, so nothing on it is
    /// believed: the address is where an answer goes, not proof of an account, and the
    /// role is what the person thought they were signing up as rather than anything the
    /// web tier would act on.
    /// </summary>
    public class AskForHelpForm
    {
        [Required(ErrorMessage = "Say who is asking.")]
        [MaxLength(150)]
        [Display(Name = "Your name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "An administrator needs an address to answer.")]
        [EmailAddress(ErrorMessage = "That is not an email address.")]
        [MaxLength(200)]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(50)]
        [Display(Name = "Role you signed up as (if you know)")]
        public string? ExpectedRole { get; set; }

        [Required(ErrorMessage = "Describe what happens when you try to sign in.")]
        [MaxLength(2000, ErrorMessage = "Keep it under 2000 characters.")]
        [Display(Name = "What is going wrong")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// A field no person filling in this form can see. The input is rendered off
        /// screen and labelled like something a script would want to complete, and a
        /// submission that fills it is a bot: this form mails every System
        /// Administrator on the system, so an unlabelled anonymous form is an
        /// invitation to use one company's SMTP against another's administrators.
        /// </summary>
        public string? Website { get; set; }
    }

    /// <summary>
    /// One open "I cannot get in" note, on the administrator's queue.
    /// </summary>
    public class HelpRequestItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ExpectedRole { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public int DaysWaiting => Math.Max(0, (int)(DateTime.UtcNow - CreatedAt).TotalDays);

        /// <summary>Today's note, which is the one that can be answered before it ages.</summary>
        public bool ArrivedToday => DaysWaiting == 0;
    }

    /// <summary>
    /// What the API said about closing a note, on 200 or on 400: a reviewer who is
    /// told only that the click failed does not know whether somebody else got there
    /// first or the note never existed.
    /// </summary>
    public class HelpOutcome
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
