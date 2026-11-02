using System;
using System.Threading.Tasks;

namespace VerichatCRM.Infrastructure.Data.External
{
    public interface IEmailService
    {
        bool IsConfigured { get; }

        Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink);

        /// <summary>
        /// Asks the recipient to prove they read mail at this address. Separate from
        /// the password-reset email on purpose: a reset says "you forgot something",
        /// a confirmation says "we have never checked this belongs to you", and the
        /// two arrive at different moments of an account's life.
        /// </summary>
        Task SendEmailConfirmationAsync(string toEmail, string displayName, string confirmationLink);

        Task SendNotificationEmailAsync(string toEmail, string subject, string message);

        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
