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
        /// <summary>
        /// Accounts sitting in one approval state, oldest first: the queue an
        /// employee works down. Only the approval gate reads it, so the ordering is
        /// the reviewer's ordering - whoever applied first is waiting longest.
        /// </summary>
        Task<IEnumerable<User>> GetByApprovalStatusAsync(string approvalStatus);
        Task<bool> EmailExistsAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<int> CountByRoleAsync(string role);
    }
}
