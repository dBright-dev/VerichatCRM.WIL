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

            // Passwords predating PBKDF2 are rewritten now that we know the
            // plaintext is correct; each legacy account upgrades on first login.
            if (PasswordHasher.NeedsUpgrade(user.PasswordHash))
            {
                user.PasswordHash = PasswordHasher.Hash(request.Password);
                await _users.UpdateAsync(user);
            }

            // The approval check runs after the password check and never before it.
            // "This account is waiting for an employee" is a fact about the address
            // that anyone could learn from the sign-up form, so handing it out before
            // the password is proved would turn the login screen into a list of who
            // has registered. Once the password is right, the person already holds
            // the account, and the reason is the useful answer.
            if (UserApproval.IsPending(user.ApprovalStatus))
                return new AuthResponse
                {
                    Success = false,
                    Message = "Your account is created and waiting for a Verichat employee to approve it. You will be able to sign in as soon as it is.",
                    User = Map(user),
                    ApprovalStatus = user.ApprovalStatus
                };

            if (UserApproval.IsRejected(user.ApprovalStatus))
                return new AuthResponse
                {
                    Success = false,
                    Message = string.IsNullOrWhiteSpace(user.RejectionReason)
                        ? "A Verichat employee reviewed this account and did not approve it. Contact Verichat if you believe that was a mistake."
                        : $"A Verichat employee reviewed this account and did not approve it: {user.RejectionReason}",
                    User = Map(user),
                    ApprovalStatus = user.ApprovalStatus
                };

            user.LastLoginAt = DateTime.UtcNow;
            await _users.UpdateAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = "Login successful.",
                Token = _jwt.GenerateToken(user),
                User = Map(user),
                ApprovalStatus = user.ApprovalStatus
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            // The role a person posts has to be one the form is allowed to hand out.
            // System Administrator is in SelfRegisterable now, so it passes here and
            // is exempted from review further down - see UserRoles.ApprovalExempt.
            if (!UserRoles.SelfRegisterable.Contains(request.Role))
                return new AuthResponse { Success = false, Message = "That role cannot be self-assigned. Ask an administrator to create the account." };

            if (await _users.EmailExistsAsync(request.Email))
                return new AuthResponse { Success = false, Message = "Email is already registered." };

            // Nothing a person says about themselves on a public form is
            // evidence. "I am a Procurement Manager" buys the stock dashboard,
            // the warehouse alerts and the logistics tracker; "I am a Client
            // User" buys a purchase order line. So the account is written down
            // as an application, and the employee who reviews it decides whether
            // it is what it claims to be. No token is issued here: the applicant
            // has a login, but the login does not open anything yet.
            // The one exception is a role named in UserRoles.ApprovalExempt -
            // System Administrator - which is recorded as Approved and therefore
            // does open the system the moment it is created.
            var approvedOnArrival = UserRoles.IsApprovalExempt(request.Role);
            var user = new User
            {
                DisplayName = request.DisplayName.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHasher.Hash(request.Password),
                Role = request.Role,
                ApprovalStatus = approvedOnArrival ? UserApproval.Approved : UserApproval.Pending
            };

            await _users.AddAsync(user);

            return new AuthResponse
            {
                Success = true,
                Message = approvedOnArrival
                    ? $"Account created as {request.Role}. This role needs no review, so sign in with the address and password you just chose."
                    : "Application received. A Verichat employee will review it, and you can sign in once it is approved. " +
                      "Check your inbox for a link to confirm this email address.",
                User = Map(user),
                ApprovalStatus = user.ApprovalStatus
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
