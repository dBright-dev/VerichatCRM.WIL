using System.Security.Cryptography;
using System.Text;

namespace VerichatCRM.API.Helpers
{
    public static class PasswordHasher
    {
        // Simple PBKDF2 hashing. For production, use BCrypt or Argon2.
        public static string Hash(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + "::verichat_salt_v1");
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string hash)
        {
            return Hash(password) == hash;
        }
    }
}
