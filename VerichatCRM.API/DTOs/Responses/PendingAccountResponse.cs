namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>
    /// One application in the approval queue.
    /// <para>
    /// <see cref="IsStaffRole"/> is the row a reviewer is actually judging. Anyone
    /// can claim "Client User" on the sign-up form and end up able to raise a
    /// purchase order, but the claim that costs the company something is the one to
    /// a staff role: those accounts open client directories, quotations, warehouse
    /// alerts and the reorder controls. The flag is computed from
    /// <see cref="Core.Entities.UserRoles.ClientAudience"/> rather than typed into
    /// the queue, so the warning cannot disagree with the routing that the web and
    /// mobile apps already use for the same role.
    /// </para>
    /// </summary>
    public class PendingAccountResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsStaffRole { get; set; }

        /// <summary>
        /// Whether the applicant clicked the link sent to the address on this row.
        /// It is not proof of who they are - the address is still self-declared and a
        /// person can confirm a mailbox they control for the occasion - but it does
        /// mean the approval or decline notice will reach them, and an applicant who
        /// never answers their own email is a different proposition to approve.
        /// </summary>
        public bool EmailVerified { get; set; }

        /// <summary>Whole days this application has been waiting, for the reviewer's ordering.</summary>
        public int DaysWaiting => Math.Max(0, (int)(DateTime.UtcNow - CreatedAt).TotalDays);
    }
}
