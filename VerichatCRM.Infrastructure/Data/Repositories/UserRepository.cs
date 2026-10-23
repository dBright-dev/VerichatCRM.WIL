using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await ((System.Linq.IQueryable<User>)_context.Users)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetByResetTokenAsync(string token)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await ((System.Linq.IQueryable<User>)_context.Users)
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User> AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountByRoleAsync(string role)
        {
            return await _context.Users.CountAsync(u => u.Role == role);
        }
    }
}
