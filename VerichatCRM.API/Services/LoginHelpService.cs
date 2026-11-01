using System.Net;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using VerichatCRM.Infrastructure.Data.External;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// Keeps the "I cannot get in" notes and tells somebody who can act.
    /// <para>
    /// Two rules shape this class. The first is that the note is never the thing in
    /// danger: the row is written and committed before any mail is attempted, and an
    /// SMTP failure is logged rather than returned, because a person who is locked
    /// out losing their message to a mail server is the exact opposite of what the
    /// screen promises. The second is that this form is anonymous and sends mail on
    /// demand, so it must not become a gateway for somebody else's inbox - hence the
    /// deduplication window, which also happens to be the right answer for the
    /// genuine case of someone pressing Send twice.
    /// </para>
    /// </summary>
    public class LoginHelpService : ILoginHelpService
    {
        /// <summary>
        /// How long a second note from the same address counts as the same problem
        /// rather than a new one. Two hours is long enough that someone re-sending
        /// because they panicked is folded into the first note, and short enough that
        /// a person still locked out the next morning does reach an administrator.
        /// </summary>
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromHours(2);

        private readonly AppDbContext _context;
        private readonly IEmailService _email;
        private readonly ILogger<LoginHelpService> _logger;

        public LoginHelpService(AppDbContext context, IEmailService email, ILogger<LoginHelpService> logger)
        {
            _context = context;
            _email = email;
            _logger = logger;
        }

        public async Task<HelpOutcome> SubmitAsync(AskForHelpRequest request)
        {
            var email = request.Email.Trim();
            // Computed out here rather than inside the predicate: SQL cannot translate
            // `DateTime.UtcNow - TimeSpan`, so the window has to arrive as a parameter.
            var since = DateTime.UtcNow - DuplicateWindow;
            var recent = await _context.LoginHelpRequests.FirstOrDefaultAsync(
                h => h.Email == email &&
                     h.Status == LoginHelpStatus.Open &&
                     h.CreatedAt >= since);

            if (recent != null)
            {
                // Honest, and not a secret: the address being quoted back is the one
                // the sender just typed into the form.
                _logger.LogInformation(
                    "Login help note from {Email} folded into the open note {Id}.", email, recent.Id);
                return new HelpOutcome(true,
                    "Your earlier note is still open with us, so there is nothing to send twice. " +
                    "An administrator will answer the address you gave.");
            }

            var note = new LoginHelpRequest
            {
                Name = request.Name.Trim(),
                Email = email,
                ExpectedRole = string.IsNullOrWhiteSpace(request.ExpectedRole)
                    ? null
                    : request.ExpectedRole.Trim(),
                Message = request.Message.Trim(),
                Channel = NoteChannel(request.Channel)
            };

            _context.LoginHelpRequests.Add(note);
            await _context.SaveChangesAsync();

            await TellAdministratorsAsync(note);

            return new HelpOutcome(true,
                "Your note is with a Verichat administrator. If the address you gave is a real " +
                "mailbox, expect an answer there.");
        }

        public async Task<List<LoginHelpResponse>> GetOpenAsync()
        {
            // Oldest first: unlike an approval queue, where a new application is a
            // fresh one, every row here is somebody who cannot work right now, and
            // the one who has waited longest is the one to read first.
            var open = await _context.LoginHelpRequests
                .Where(h => h.Status == LoginHelpStatus.Open)
                .OrderBy(h => h.CreatedAt)
                .ToListAsync();

            return open.Select(ToResponse).ToList();
        }

        public async Task<HelpOutcome> ResolveAsync(string id, string resolverUserId)
        {
            var note = await _context.LoginHelpRequests.FirstOrDefaultAsync(h => h.Id == id);
            if (note == null)
                return new HelpOutcome(false, "That note is no longer in the database.");

            if (note.Status == LoginHelpStatus.Resolved)
                return new HelpOutcome(false, "That note was already closed. Open a new one if it has come back.");

            note.Status = LoginHelpStatus.Resolved;
            note.ResolvedByUserId = resolverUserId;
            note.ResolvedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return new HelpOutcome(true, $"Closed the note from {note.Email}.");
        }

        /// <summary>
        /// Mails the note to the accounts that can actually do something about it.
        /// <para>
        /// The recipients are read from the user table rather than from a configured
        /// address, because there is no address in this system that means
        /// "administrator" - the Bootstrap admin is a credential, not a mailbox, and a
        /// support alias would be a second place to keep the same fact. Every active,
        /// approved System Administrator is a person who can approve an application,
        /// change a role or deactivate an account, which is the whole list of fixes a
        /// sign-in problem needs. On a brand-new install the query is empty, the note
        /// stays in the queue, and the log says so; losing the mail is not the same as
        /// losing the note.
        /// </para>
        /// </summary>
        private async Task TellAdministratorsAsync(LoginHelpRequest note)
        {
            var recipients = await _context.Users
                .Where(u => u.Role == UserRoles.SystemAdministrator && u.IsActive
                            && u.ApprovalStatus == UserApproval.Approved)
                .Select(u => u.Email)
                .ToListAsync();

            if (recipients.Count == 0)
            {
                _logger.LogWarning(
                    "Login help note {Id} from {Email} has no administrator to notify.", note.Id, note.Email);
                return;
            }

            var body =
                $"<p><strong>{WebUtility.HtmlEncode(note.Name)}</strong> ({WebUtility.HtmlEncode(note.Email)}) " +
                $"cannot sign in from the {WebUtility.HtmlEncode(note.Channel)}." +
                (note.ExpectedRole == null
                    ? "</p>"
                    : $" They signed up as <strong>{WebUtility.HtmlEncode(note.ExpectedRole)}</strong>.</p>") +
                $"<p>{BreakLines(WebUtility.HtmlEncode(note.Message))}</p>" +
                "<p>Read their application in the account approvals queue, or check the address on the users screen.</p>";

            foreach (var recipient in recipients)
            {
                try
                {
                    await _email.SendAsync(recipient, "Someone cannot sign in to Verichat", body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Login help note {Id} was recorded but the notice to {Email} failed.", note.Id, recipient);
                }
            }

            _logger.LogInformation(
                "Login help note {Id} from {From} sent to {Count} administrator(s).",
                note.Id, note.Email, recipients.Count);
        }

        /// <summary>
        /// A note is typed as prose, and an email that swallows the line breaks is
        /// harder to read than the form it came from. Encoding happens first, so this
        /// only ever replaces characters the caller cannot control.
        /// </summary>
        private static string BreakLines(string encoded) =>
            encoded.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");

        /// <summary>
        /// Two words, whatever the caller sent. The column only ever means "a browser"
        /// or "a phone", so anything else falls back to Web rather than putting a third
        /// or fourth spelling of the same thing in front of a reviewer.
        /// </summary>
        private static string NoteChannel(string? channel) =>
            string.Equals(channel?.Trim(), "Mobile", StringComparison.OrdinalIgnoreCase) ? "Mobile" : "Web";

        private static LoginHelpResponse ToResponse(LoginHelpRequest h) => new()
        {
            Id = h.Id,
            Name = h.Name,
            Email = h.Email,
            ExpectedRole = h.ExpectedRole,
            Message = h.Message,
            Channel = h.Channel,
            CreatedAt = h.CreatedAt,
            Status = h.Status,
            ResolvedByUserId = h.ResolvedByUserId,
            ResolvedAt = h.ResolvedAt
        };
    }
}
