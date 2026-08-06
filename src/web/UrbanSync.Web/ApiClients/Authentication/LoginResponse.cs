using UrbanSync.Web.ApiClients.Users;

namespace UrbanSync.Web.ApiClients.Authentication;

public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public UserResponse User { get; set; } = new();
}