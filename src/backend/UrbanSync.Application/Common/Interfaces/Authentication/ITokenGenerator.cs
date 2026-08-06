namespace UrbanSync.Application.Common.Interfaces.Authentication;

public interface ITokenGenerator
{
    GeneratedToken Generate(
        int userId,
        string fullName,
        string email,
        string role);
}

public sealed record GeneratedToken(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);