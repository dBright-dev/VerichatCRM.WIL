using System;
using System.ComponentModel.DataAnnotations;

namespace VerichatCRM.Core.Entities
{
    public class User
    {
        [Key]
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Procurement Manager"; // stored as string for simplicity
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        /// <summary>One-time token issued by the forgot-password flow.</summary>
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }
    }
}
