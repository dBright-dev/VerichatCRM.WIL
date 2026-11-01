using System.Security.Cryptography;
using System.Text;

namespace VerichatCRM.API.Helpers
{
    /// <summary>
    /// Single-purpose bearer tokens for password reset and email confirmation.
    ///
    /// The plaintext is emailed; only its SHA-256 is stored. A reset token IS a login
    /// for the hour it lives - whoever presents it chooses a new password - so a read
    /// of the Users table that was never meant to hand out accounts must not be able
    /// to. Storing the raw token would do exactly that, and would also leave a working
    /// credential sitting on the row after the account changes hands.
    ///
    /// Lookup is by hash rather than a constant-time compare of a submitted token:
    /// the guess space is 128 bits of randomness, the query is indexed, and the only
    /// timing signal an attacker gets is the database's, which says nothing about the
    /// token beyond whether the whole thing matched.
    /// </summary>
    public static class RecoveryToken
    {
        public static string Create() =>
            Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

        /// <summary>
        /// The value stored in and queried from the token columns. Lower-casing first
        /// makes the link case-insensitive, because mail clients rewrap and reprint
        /// URLs and nobody should have to notice that a pasted link changed case.
        /// </summary>
        public static string Hash(string token) =>
            string.IsNullOrWhiteSpace(token)
                ? string.Empty
                : Convert.ToBase64String(SHA256.HashData(
                    Encoding.UTF8.GetBytes(token.Trim().ToLowerInvariant())));
    }
}
