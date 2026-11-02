using System.Net;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Infrastructure.Data;
using VerichatCRM.Infrastructure.Data.External;

namespace VerichatCRM.API.Services
{
    /// <summary>
    /// The public contact form's door into the same enquiry queue a representative works.
    /// <para>
    /// Three rules shape this class, and they are the same three the sign-in help notes
    /// were built on. First, the enquiry is never the thing in danger: it is written and
    /// committed by <see cref="InquiryService"/> before any mail is attempted, and an SMTP
    /// failure is logged rather than returned - a company that loses a prospective client's
    /// message because its mail server was having a morning has built a worse problem than
    /// the one the form was meant to solve.
    /// </para>
    /// <para>
    /// Second, the form is anonymous and reaches a real mailbox, so it must not become a
    /// relay for somebody else's correspondence. The recipient is a configured address of
    /// ours and never anything the caller supplied, and one open enquiry per address per
    /// window folds a panicked second Send - and a scripted second thousand - into the note
    /// the desk already has.
    /// </para>
    /// <para>
    /// Third, an anonymous submission is not allowed to invent relationships. Nothing here
    /// attaches the enquiry to a client account or sets its owner: those are decisions a
    /// person makes in the queue, with the sector the visitor typed in only as a label on
    /// the note.
    /// </para>
    /// </summary>
    public class PublicEnquiryService : IPublicEnquiryService
    {
        /// <summary>
        /// How long one address may hold one open enquiry. Two hours matches the sign-in
        /// help window: long enough that re-sending out of frustration is the same note,
        /// short enough that a client still waiting tomorrow morning does reach the desk
        /// again.
        /// </summary>
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromHours(2);

        private readonly AppDbContext _context;
        private readonly IInquiryService _inquiries;
        private readonly IEmailService _email;
        private readonly IConfiguration _config;
        private readonly ILogger<PublicEnquiryService> _logger;

        public PublicEnquiryService(
            AppDbContext context,
            IInquiryService inquiries,
            IEmailService email,
            IConfiguration config,
            ILogger<PublicEnquiryService> logger)
        {
            _context = context;
            _inquiries = inquiries;
            _email = email;
            _config = config;
            _logger = logger;
        }

        public async Task<PublicEnquiryOutcome> SubmitAsync(PublicEnquiryRequest request)
        {
            var email = request.Email.Trim();
            var subject = request.Subject.Trim();

            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return new PublicEnquiryOutcome(false,
                    "There is not enough in the message for us to act on. Say what you need and we will answer it.");
            }

            // Computed outside the predicate: SQL cannot translate `DateTime.UtcNow -
            // TimeSpan`, so the window has to reach the query as a parameter.
            var since = DateTime.UtcNow - DuplicateWindow;
            var openStatuses = InquiryStatuses.Open;
            var existing = await _context.Inquiries.FirstOrDefaultAsync(i =>
                i.ContactEmail == email &&
                openStatuses.Contains(i.Status) &&
                i.CreatedAt >= since);

            if (existing != null)
            {
                _logger.LogInformation(
                    "Public enquiry from {Email} folded into the open enquiry {Id}.", email, existing.InquiryId);
                return new PublicEnquiryOutcome(true,
                    "We already have this enquiry from you and it is still open, so there is nothing to send twice. " +
                    "A Verichat representative will answer the address you gave.",
                    Reference(existing.InquiryId));
            }

            var created = await _inquiries.CreateAsync(new CreateInquiryRequest
            {
                ContactName = request.Name.Trim(),
                ContactEmail = email,
                ContactPhone = request.Phone,
                Company = string.IsNullOrWhiteSpace(request.Organisation) ? null : request.Organisation.Trim(),
                Subject = subject,
                // The sector a visitor chooses is a label for the desk, not a client
                // record, so it travels in the note rather than in a column it cannot
                // legitimately occupy.
                Description = Describe(request),
                Source = InquirySources.Website,
                Urgency = request.Urgency,
                // No ClientId, no owner, no score: the queue makes those judgements.
            });

            await TellSalesAsync(request, created.InquiryId, created.CreatedAt);

            return new PublicEnquiryOutcome(true,
                "Your enquiry is with the sales desk. Expect an answer at " + email +
                " within one working day.",
                Reference(created.InquiryId));
        }

        /// <summary>
        /// The note as the desk reads it: what was typed, then who typed it and where
        /// from. Line breaks are kept because the message field is a paragraph box.
        /// </summary>
        private static string Describe(PublicEnquiryRequest request)
        {
            var tail = "Enquiry from the public " +
                       (string.Equals(request.Channel?.Trim(), "Mobile", StringComparison.OrdinalIgnoreCase)
                           ? "contact form on the mobile app"
                           : "contact page") + ".";

            if (!string.IsNullOrWhiteSpace(request.Sector))
                tail += " Sector given: " + request.Sector.Trim() + ".";

            return request.Message.Trim() + Environment.NewLine + Environment.NewLine + tail;
        }

        /// <summary>
        /// The mailbox an enquiry is copied to unless <c>Email:SalesAddress</c> says
        /// otherwise. The address is published on the site anyway, so keeping the default
        /// here costs nothing, and it means a deployment that never sets the key still
        /// reaches a person rather than silently filing enquiries nobody reads.
        /// <para>
        /// What is deliberately not configurable is the caller's influence over it: the
        /// recipient is ours and never anything from the request, because an anonymous
        /// form that mails a supplied address is a mail relay with a friendly face.
        /// </para>
        /// </summary>
        private const string DefaultSalesAddress = "sales@verichat.co.zw";

        /// <summary>
        /// Mails a copy of the enquiry to the sales address.
        /// <para>
        /// A blank address is treated as a misconfiguration and logged, not swallowed: an
        /// enquiry is still filed when nobody was told about it, which is the whole point
        /// of writing the row before the mail.
        /// </para>
        /// </summary>
        private async Task TellSalesAsync(PublicEnquiryRequest request, string inquiryId, DateTime receivedAt)
        {
            var sales = string.IsNullOrWhiteSpace(_config["Email:SalesAddress"])
                ? DefaultSalesAddress
                : _config["Email:SalesAddress"]!.Trim();

            var body =
                $"<p><strong>{WebUtility.HtmlEncode(request.Name.Trim())}</strong> " +
                $"({WebUtility.HtmlEncode(request.Email.Trim())}) sent an enquiry from the public contact page." +
                "</p>" +
                Field("Telephone", request.Phone) +
                Field("Organisation", request.Organisation) +
                Field("Sector", request.Sector) +
                Field("Urgency", request.Urgency) +
                Field("Received", receivedAt.ToString("yyyy-MM-dd HH:mm") + " UTC") +
                Field("Reference", inquiryId) +
                $"<p><strong>Subject:</strong> {WebUtility.HtmlEncode(request.Subject.Trim())}</p>" +
                $"<p>{BreakLines(WebUtility.HtmlEncode(request.Message.Trim()))}</p>" +
                "<p>Open it in the enquiry queue - it is filed as a Website enquiry and scored like any other.</p>";

            try
            {
                await _email.SendAsync(sales, "New enquiry: " + request.Subject.Trim(), body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Public enquiry {Id} was recorded but the notice to {Email} failed.", inquiryId, sales);
                return;
            }

            // EmailService answers quietly when SMTP is not configured - it logs the
            // message it would have sent and returns. Saying "notified" in that case
            // would be the one untrue line in this file, so the log says what actually
            // happened: the enquiry exists, and nobody has been told yet.
            if (_email.IsConfigured)
            {
                _logger.LogInformation("Public enquiry {Id} from {From} notified {Sales}.",
                    inquiryId, request.Email, sales);
            }
            else
            {
                _logger.LogWarning(
                    "Public enquiry {Id} from {From} was filed, but SMTP is not configured so the sales desk was not told.",
                    inquiryId, request.Email);
            }
        }

        private static string Field(string label, string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : $"<p><strong>{label}:</strong> {WebUtility.HtmlEncode(value.Trim())}</p>";

        /// <summary>
        /// A GUID is not something a caller can read down a telephone. The first eight
        /// characters of the enquiry's id are unique enough for a queue of this size and
        /// are a prefix of the primary key, so whoever takes the call can find the row.
        /// </summary>
        private static string Reference(string inquiryId) =>
            "VC-" + inquiryId.Replace("-", string.Empty).Substring(0, 8).ToUpperInvariant();

        private static string BreakLines(string encoded) =>
            encoded.Replace("\r\n", "<br />").Replace("\n", "<br />").Replace("\r", "<br />");
    }
}
