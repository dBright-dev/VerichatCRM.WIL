using VerichatCRM.Infrastructure.Data.External;

namespace VerichatCRM.Tests;

/// <summary>
/// An SMTP transport that remembers what it was asked to send. The approval tests
/// read it to check the applicant is told either way, without a mail server.
/// </summary>
internal sealed class FakeEmailService : IEmailService
{
    public List<(string To, string Subject, string Body)> Sent { get; } = [];

    /// <summary>When true, every send fails - the case a decision must survive.</summary>
    public bool FailOnSend { get; set; }

    public bool IsConfigured => true;

    public Task SendPasswordResetEmailAsync(string toEmail, string displayName, string resetLink) =>
        SendAsync(toEmail, "Reset your Verichat password", resetLink);

    public Task SendEmailConfirmationAsync(string toEmail, string displayName, string confirmationLink) =>
        SendAsync(toEmail, "Confirm your email address with Verichat", confirmationLink);

    public Task SendNotificationEmailAsync(string toEmail, string subject, string message) =>
        SendAsync(toEmail, subject, message);

    public Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (FailOnSend) throw new InvalidOperationException("SMTP is down.");
        Sent.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
