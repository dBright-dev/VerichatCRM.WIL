using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Enums;

namespace VerichatCRM.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public ICollection<Client> AssignedClients { get; set; } = new List<Client>();
        public ICollection<Deal> AssignedDeals { get; set; } = new List<Deal>();
    }
}
