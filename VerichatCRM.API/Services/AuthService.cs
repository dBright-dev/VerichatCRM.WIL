using VerichatCRM.API.DTOs.Requests;
using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.API.Helpers;
using VerichatCRM.API.Services.Interfaces;
using VerichatCRM.Core.Entities;
using VerichatCRM.Core.Interfaces;

namespace VerichatCRM.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly JwtHelper _jwt;

        public AuthService(IUserRepository users, JwtHelper jwt)
        {
            _users = users;
            _jwt = jwt;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _users.GetByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return new AuthResponse { Success = false, Message = "Invalid credentials." };

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                return new AuthResponse { Success = false, Message = "Invalid credentials." };

            user.LastLoginAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = _jwt.GenerateToken(user),
                User = Map(user)
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (!UserRoles.All.Contains(request.Role))
                return new AuthResponse { Success = false, Message = "Invalid role specified." };

            if (await _users.EmailExistsAsync(request.Email))
                return new AuthResponse { Success = false, Message = "Email is already registered." };

            var user = new User
            {
                DisplayName = request.DisplayName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(request.Password),
                Role = request.Role
            };

            await _users.AddAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Registration successful.",
                Token = _jwt.GenerateToken(user),
                User = Map(user)
            };
        }

        private static UserInfo Map(User u) => new()
        {
            UserId = u.UserId,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Role = u.Role
        };
    }
}
