namespace UrbanSync.Application.Common.Interfaces.Authentication;

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt) Hash(string password);

    bool Verify(
        string password,
        byte[] passwordHash,
        byte[] passwordSalt);
}