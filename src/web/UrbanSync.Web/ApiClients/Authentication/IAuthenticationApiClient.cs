using UrbanSync.Web.ApiClients.Users;

namespace UrbanSync.Web.ApiClients.Authentication;

public interface IAuthenticationApiClient
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<UserResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}