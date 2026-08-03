using System.Security.Cryptography;
using System.Text;
using UrbanSync.Application.Common.Interfaces.Authentication;

namespace UrbanSync.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    public (byte[] Hash, byte[] Salt) Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        using var hmac = new HMACSHA512();

        var salt = hmac.Key;
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return (hash, salt);
    }

    public bool Verify(
        string password,
        byte[] passwordHash,
        byte[] passwordSalt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(passwordSalt);

        using var hmac = new HMACSHA512(passwordSalt);

        var computedHash = hmac.ComputeHash(
            Encoding.UTF8.GetBytes(password));

        return CryptographicOperations.FixedTimeEquals(
            computedHash,
            passwordHash);
    }
}