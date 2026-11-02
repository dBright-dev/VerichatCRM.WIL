using System;
using System.Collections.Generic;
using System.Text;
using VerichatCRM.Core.Entities;

namespace VerichatCRM.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(string userId);
        Task<IEnumerable<User>> GetAllAsync();
        /// <summary>
        /// The account holding this <b>hashed</b> reset token. Callers hash what the
        /// user presented (see <c>RecoveryToken.Hash</c>) because the plaintext only
        /// ever exists in the email and in the link.
        /// </summary>
        Task<User?> GetByResetTokenAsync(string tokenHash);
        /// <summary>The account holding this <b>hashed</b> email-confirmation token.</summary>
        Task<User?> GetByVerificationTokenAsync(string tokenHash);
        Task<bool> EmailExistsAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<int> CountByRoleAsync(string role);
    }
}
