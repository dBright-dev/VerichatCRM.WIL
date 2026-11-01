using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.API.DTOs.Requests
{
    /// <summary>
    /// What a visitor types into the public contact form.
    /// <para>
    /// It is a superset of nothing and a subset of <see cref="CreateInquiryRequest"/>:
    /// the fields a person at a desk would give the sales team, and none of the fields
    /// only a signed-in representative can set. There is deliberately no ClientId, no
    /// score and no owner here - an anonymous visitor cannot nominate the account an
    /// enquiry belongs to, and a number they type into a "value" box is not the
    /// estimated value of a deal.
    /// </para>
    /// <para>
    /// Nothing on this form is believed. The email address is where an answer goes, not
    /// proof of anything, and the sector is what the visitor called themselves.
    /// </para>
    /// </summary>
    public class PublicEnquiryRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? Phone { get; set; }

        [MaxLength(160)]
        public string? Organisation { get; set; }

        /// <summary>Free text, and only ever a label for the desk - never a foreign key.</summary>
        [MaxLength(80)]
        public string? Sector { get; set; }

        [Required, MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>Immediate | High | Standard | Flexible; anything else is treated as unset.</summary>
        [MaxLength(30)]
        public string? Urgency { get; set; }

        /// <summary>Which front end sent it, so a queue can tell a browser from a phone.</summary>
        [MaxLength(40)]
        public string? Channel { get; set; }
    }
}
