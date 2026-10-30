using System.Threading.Tasks;

namespace VerichatCRM.Infrastructure.Data.External.Interfaces
{
    /// <summary>Outbound email (password resets, notifications).</summary>
    public interface IEmailService
    {
        bool IsConfigured { get; }

        Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink);

        Task SendNotificationEmailAsync(string toEmail, string subject, string message);
    }
}
