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
        private readonly IMobileService _mobile;
        private readonly JwtHelper _jwt;

        public AuthService(IUserRepository users, IMobileService mobile, JwtHelper jwt)
        {
            _users = users;
            _mobile = mobile;
            _jwt = jwt;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _users.GetByEmailAsync(request.Email);

            if (user == null || !user.IsActive)
                return new AuthResponse { Success = false, Message = "Invalid credentials." };

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                return new AuthResponse { Success = false, Message = "Invalid credentials." };

            // Passwords predating PBKDF2 are rewritten now that we know the
            // plaintext is correct; each legacy account upgrades on first login.
            if (PasswordHasher.NeedsUpgrade(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(request.Password);
                await _users.UpdateAsync(user);
            }

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
            // The role a person posts has to be one the form is allowed to hand out,
            // and that set is now only the two client-facing roles. It is the whole
            // of the access control on this form: the account is live the moment it
            // is written and nobody reviews it, so the form must not be able to
            // award a staff login. "I am a Procurement Manager" used to buy a place
            // in a queue and the stock dashboard as soon as someone read it; now it
            // buys a refusal, and a staff account comes from an employee at
            // /Admin/Users instead. Refusing at the door is shorter than recording a
            // claim and waiting for somebody to disbelieve it.
            if (!UserRoles.SelfRegisterable.Contains(request.Role))
                return new AuthResponse
                {
                    Success = false,
                    Message = "The sign-up form opens a client or student account. A staff account is " +
                              "created by a Verichat employee - use the contact page and ask for one."
                };

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

            // Orders, quotations and the enquiry queue are keyed on Clients.ClientId,
            // which is the UserId. That row used to be written when an employee
            // accepted the application; with no review step it is written here, or the
            // person signs in to a portal with nothing to order against.
            if (UserRoles.ClientAudience.Contains(user.Role))
                await _mobile.EnsureClientAccountAsync(user.UserId);

            // No token here on purpose: the web and mobile sign-up screens both hold
            // the password the person just chose and sign them in with it, so the
            // session is issued by the one code path that verifies a password rather
            // than by a second copy of the minting rule.
            return new AuthResponse
            {
                Success = true,
                Message = "Your account is ready. Check your inbox for a link to confirm this address.",
                User = Map(user)
            };
        }

        /// <summary>
        /// Creates an account on an administrator's instruction. The only difference
        /// from <see cref="RegisterAsync"/> is who vouched for the role: here a signed-in
        /// System Administrator named the person, so all seven roles are on the table -
        /// including the five the public form refuses, which is how a staff login exists
        /// at all now that self-registration cannot ask for one.
        /// </summary>
        public async Task<AuthResponse> CreateAccountByAdminAsync(RegisterRequest request)
        {
            if (!UserRoles.All.Contains(request.Role))
                return new AuthResponse { Success = false, Message = "Select a valid role." };

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

            if (UserRoles.ClientAudience.Contains(user.Role))
                await _mobile.EnsureClientAccountAsync(user.UserId);

            return new AuthResponse
            {
                Success = true,
                Message = $"{user.DisplayName} can sign in with this password now.",
                User = Map(user)
            };
        }

        private static UserInfo Map(User u) => new()
        {
            UserId = u.UserId,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Role = u.Role,
            EmailVerified = u.EmailVerified
        };
    }
}
