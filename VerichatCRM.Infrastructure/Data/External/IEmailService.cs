using System;
using System.Threading.Tasks;

namespace VerichatCRM.Infrastructure.Data.External
{
    public interface IEmailService
    {
        bool IsConfigured { get; }

        Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink);

        Task SendNotificationEmailAsync(string toEmail, string subject, string message);

        Task SendAsync(string toEmail, string subject, string htmlBody);
    }
}
