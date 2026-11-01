namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>
    /// One "I cannot get in" note, as the administrator's queue shows it.
    /// </summary>
    public class LoginHelpResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ExpectedRole { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>Web or Mobile - which sign-in screen the note came from.</summary>
        public string Channel { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }

        /// <summary>
        /// Whole days this person has been locked out, for the reviewer's ordering.
        /// A sign-in problem is the one queue where "waiting since yesterday" is the
        /// whole point: the person is not waiting for a formality, they cannot work.
        /// </summary>
        public int DaysWaiting => Math.Max(0, (int)(DateTime.UtcNow - CreatedAt).TotalDays);
    }
}
