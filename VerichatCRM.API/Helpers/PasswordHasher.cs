using System.Security.Cryptography;
using System.Text;

namespace VerichatCRM.API.Helpers
{
    /// <summary>
    /// PBKDF2-SHA256 with a random per-password salt, stored as
    /// <c>pbkdf2-sha256$iterations$salt$hash</c>.
    ///
    /// Verify still accepts the previous format - unsalted single-round SHA256 over
    /// password + a hard-coded global salt - so existing accounts keep signing in.
    /// Callers must re-hash on success when <see cref="NeedsUpgrade"/> is true, which
    /// migrates each account to PBKDF2 the first time its owner logs in.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Prefix = "pbkdf2-sha256";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 210_000;

        // The pre-PBKDF2 format's global salt. Kept only so old hashes can be checked.
        private const string LegacyPepper = "::verichat_salt_v1";

        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

            return string.Concat(
                Prefix, "$",
                Iterations.ToString(), "$",
                Convert.ToBase64String(salt), "$",
                Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            if (!storedHash.StartsWith(Prefix + "$", StringComparison.Ordinal))
                return VerifyLegacy(password, storedHash);

            try
            {
                var parts = storedHash.Split('$');
                if (parts.Length != 4 || parts[0] != Prefix) return false;
                if (!int.TryParse(parts[1], out var iterations) || iterations <= 0) return false;

                var salt = Convert.FromBase64String(parts[2]);
                var expected = Convert.FromBase64String(parts[3]);

                var actual = Rfc2898DeriveBytes.Pbkdf2(
                    password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// True when the stored hash predates PBKDF2 and should be replaced on the
        /// next successful login.
        /// </summary>
        public static bool NeedsUpgrade(string storedHash) =>
            !string.IsNullOrEmpty(storedHash)
            && !storedHash.StartsWith(Prefix + "$", StringComparison.Ordinal);

        private static bool VerifyLegacy(string password, string storedHash)
        {
            try
            {
                var actual = SHA256.HashData(Encoding.UTF8.GetBytes(password + LegacyPepper));
                var expected = Convert.FromBase64String(storedHash);
                return expected.Length == actual.Length
                       && CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
