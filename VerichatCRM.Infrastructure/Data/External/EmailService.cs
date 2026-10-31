using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace VerichatCRM.Infrastructure.Data.External
{
    /// <summary>
    /// SMTP email delivery. Configuration is read from the "Email" section:
    /// <code>
    /// "Email": { "Host": "", "Port": 587, "UseSsl": true,
    ///            "Username": "", "Password": "", "FromAddress": "", "FromName": "" }
    /// </code>
    /// When no host is configured the message is logged instead of sent, so
    /// password-reset and notification flows keep working in development.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_config["Email:Host"]);

        public Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink)
        {
            var subject = "Reset your Verichat password";
            var body = "<h2>Password reset</h2>"
                + "<p>Hi " + (string.IsNullOrWhiteSpace(displayName) ? "there" : displayName) + ",</p>"
                + "<p>We received a request to reset your Verichat Quality Projects password.</p>"
                + "<p><a href=\"" + resetLink + "\">Click here to choose a new password</a></p>"
                + "<p>This link expires in 1 hour. If you did not request a reset you can ignore this email.</p>";

            return SendAsync(toEmail, subject, body);
        }

        public Task SendNotificationEmailAsync(string toEmail, string subject, string message) =>
            SendAsync(toEmail, subject, "<p>" + message + "</p>");

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (!IsConfigured)
            {
                _logger.LogInformation(
                    "Email not configured - would send {Subject} to {To}. Body: {Body}",
                    subject, toEmail, htmlBody);
                return;
            }

            var host = _config["Email:Host"]!;
            var port = int.TryParse(_config["Email:Port"], out var p) ? p : 587;
            var useSsl = !bool.TryParse(_config["Email:UseSsl"], out var ssl) || ssl;
            var username = _config["Email:Username"];
            var password = _config["Email:Password"];
            var from = _config["Email:FromAddress"] ?? "no-reply@verichat.co.za";
            var fromName = _config["Email:FromName"] ?? "Verichat Quality Projects";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                Credentials = string.IsNullOrWhiteSpace(username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(username, password)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
            _logger.LogInformation("Sent {Subject} to {To}", subject, toEmail);
        }
    }
}
