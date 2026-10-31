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
        Task<User?> GetByResetTokenAsync(string token);
        Task<bool> EmailExistsAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<int> CountByRoleAsync(string role);
    }
}
