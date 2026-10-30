using System;
using System.Collections.Generic;
using System.Text;

namespace VerichatCRM.API.DTOs.Responses
{
    /// <summary>One user row as returned to administrators.</summary>
    public class UserListItemResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class AuditLogResponse
    {
        public string AuditId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}