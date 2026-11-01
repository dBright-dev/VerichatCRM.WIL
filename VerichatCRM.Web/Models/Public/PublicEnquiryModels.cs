using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Web.Models.Public
{
    /// <summary>
    /// The contact page's enquiry form. Anonymous, so nothing typed into it is
    /// believed: the sector is a label for the desk rather than a client record, and
    /// the address is where an answer goes rather than proof of anybody's identity.
    /// The field list is deliberately short of the things a representative decides -
    /// no client, no owner, no score.
    /// </summary>
    public class PublicEnquiryForm
    {
        [Required(ErrorMessage = "Say who is asking.")]
        [MaxLength(150)]
        [Display(Name = "Your name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "We need an address to answer.")]
        [EmailAddress(ErrorMessage = "That is not an email address.")]
        [MaxLength(200)]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(40)]
        [Display(Name = "Telephone (optional)")]
        public string? Phone { get; set; }

        [MaxLength(160)]
        [Display(Name = "Organisation")]
        public string? Organisation { get; set; }

        [MaxLength(80)]
        [Display(Name = "What you need it for")]
        public string? Sector { get; set; }

        [Required(ErrorMessage = "Give the enquiry a subject.")]
        [MaxLength(200)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Say what you need, and we will answer it.")]
        [MaxLength(2000, ErrorMessage = "Keep it under 2000 characters.")]
        [Display(Name = "Your enquiry")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "How soon")]
        public string? Urgency { get; set; }

        /// <summary>
        /// A field no reader of this page was shown. It is rendered off screen and
        /// labelled like something a script would want to complete, and a submission
        /// that fills it is a bot. This form writes to the live enquiry queue and
        /// copies a mailbox, so an unguarded anonymous form is an invitation to use
        /// one company's mail server against another's.
        /// </summary>
        public string? Website { get; set; }
    }

    /// <summary>
    /// What the API said about a submission, on 200 or on 400. The reference is the
    /// enquiry's own short id: the sentence a caller reads down the telephone.
    /// </summary>
    public class PublicEnquiryResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    /// <summary>
    /// The screen shown after a submission: what happened, the address an answer goes to,
    /// and the reference if the queue produced one.
    /// <para>
    /// A bot that filled the hidden field is answered with this same screen with the
    /// reference left out. Saying "we have it" to a script is the point of the trap, and
    /// inventing an id would be the one lie on an otherwise honest page - a caller who
    /// reads a reference over the telephone gets nothing when the desk searches for it.
    /// </para>
    /// </summary>
    public class ContactReceiptViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }

    /// <summary>
    /// One catalogue row as the public gallery may show it. This is a projection of
    /// <see cref="VerichatCRM.Core.Entities.Product"/> that the API builds field by field,
    /// and the fields left out are the point: price, stock on hand and the reorder
    /// threshold belong to the business, and a page a competitor can read without signing
    /// in does not carry them.
    /// </summary>
    public class PublicProductItem
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
