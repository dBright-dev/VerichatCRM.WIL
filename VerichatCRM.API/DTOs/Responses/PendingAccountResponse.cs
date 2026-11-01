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

        /// <summary>Whole days this application has been waiting, for the reviewer's ordering.</summary>
        public int DaysWaiting => Math.Max(0, (int)(DateTime.UtcNow - CreatedAt).TotalDays);
    }
}
