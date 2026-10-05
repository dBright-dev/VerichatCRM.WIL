using VerichatCRM.API.DTOs.Responses;
using VerichatCRM.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace VerichatCRM.API.Services
{
    public partial class MobileService
    {
        public async Task<MobileProfileResponse> GetProfileAsync(string userId)
        {
            var user = await _users.GetByIdAsync(userId);
            var profile = await _db.MobileProfiles.FindAsync(userId);
            var orders = await _db.Orders.CountAsync(o => o.ClientId == userId);

            return new MobileProfileResponse
            {
                FullName = user?.DisplayName ?? "Client",
                OrderCount = orders,
                MemberSince = user?.CreatedAt ?? DateTime.UtcNow,
                Email = user?.Email ?? string.Empty,
                Phone = profile?.Phone ?? string.Empty,
                Company = profile?.Company ?? string.Empty,
                VatNumber = profile?.VatNumber,
                DefaultAddress = profile == null ? null : new MobileAddressResponse
                {
                    Name = profile.AddressName,
                    Street = profile.Street,
                    City = profile.City,
                    Country = profile.Country
                }
            };
        }

        public async Task<MobileProfileResponse> UpdateProfileAsync(
            string userId, Action<User> updateUser, Action<MobileProfile> updateProfile)
        {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");
            updateUser(user);
            await _users.UpdateAsync(user);

            var profile = await _db.MobileProfiles.FindAsync(userId);
            if (profile == null)
            {
                profile = new MobileProfile { UserId = userId };
                _db.MobileProfiles.Add(profile);
            }
            updateProfile(profile);
            profile.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return await GetProfileAsync(userId);
        }

        public async Task<MobileSettingsResponse> GetSettingsAsync(string userId)
        {
            var user = await _users.GetByIdAsync(userId);
            var settings = await _db.MobileSettings.FindAsync(userId)
                ?? new MobileSettings { UserId = userId };
            return new MobileSettingsResponse
            {
                FullName = user?.DisplayName ?? "Client",
                Preferences = new MobilePreferencesResponse
                {
                    PushNotifications = settings.PushNotifications,
                    BiometricLogin = settings.BiometricLogin,
                    DarkMode = settings.DarkMode
                }
            };
        }

        public async Task<MobileSettingsResponse> UpdateSettingsAsync(
            string userId, bool? push, bool? biometric, bool? dark)
        {
            var settings = await _db.MobileSettings.FindAsync(userId);
            if (settings == null)
            {
                settings = new MobileSettings { UserId = userId };
                _db.MobileSettings.Add(settings);
            }
            if (push.HasValue) settings.PushNotifications = push.Value;
            if (biometric.HasValue) settings.BiometricLogin = biometric.Value;
            if (dark.HasValue) settings.DarkMode = dark.Value;
            settings.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await GetSettingsAsync(userId);
        }

        private static string DefaultGoalName(string type) => type switch
        {
            "steps" => "Steps",
            "water" => "Water",
            "sleep" => "Sleep",
            "exercise" => "Exercise",
            _ => type
        };

        private static double DefaultGoalTarget(string type) => type switch
        {
            "steps" => 10000,
            "water" => 2,
            "sleep" => 8,
            "exercise" => 30,
            _ => 1
        };

        private static string DefaultGoalUnit(string type) => type switch
        {
            "steps" => "steps",
            "water" => "L",
            "sleep" => "hrs",
            "exercise" => "min",
            _ => ""
        };

        /// <summary>
        /// Orders/quotations reference Clients.ClientId, so every mobile user
        /// needs a matching Client row (keyed by their UserId). Creates it on
        /// first use if registration didn't already.
        /// </summary>
        public async Task EnsureClientAccountAsync(string userId)
        {
            var exists = await _db.Clients.AnyAsync(c => c.ClientId == userId);
            if (exists) return;

            var user = await _users.GetByIdAsync(userId);
            var profile = await _db.MobileProfiles.FindAsync(userId);

            _db.Clients.Add(new Client
            {
                ClientId = userId,
                ClientName = string.IsNullOrWhiteSpace(profile?.Company)
                    ? (user?.DisplayName ?? "Mobile Client")
                    : profile!.Company,
                Sector = "Clinical",
                Address = profile == null ? string.Empty : $"{profile.Street}, {profile.City}, {profile.Country}".Trim(','),
                ContactPerson = user?.DisplayName ?? string.Empty,
                ContactEmail = user?.Email ?? string.Empty,
                ContactPhone = profile?.Phone ?? string.Empty,
                ContractStatus = "Active"
            });
            await _db.SaveChangesAsync();
        }
    }
}
