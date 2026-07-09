using System.Security.Cryptography;

namespace UrbanSync.Business.Helpers
{
    public static class PasswordHasher
    {
        public static (byte[] hash, byte[] salt) Hash(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return (hash, salt);
        }

        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
}
