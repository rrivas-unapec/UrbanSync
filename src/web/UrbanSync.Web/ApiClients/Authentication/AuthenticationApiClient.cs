using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Users;

namespace UrbanSync.Web.ApiClients.Authentication;

public sealed class AuthenticationApiClient
    : ApiClientBase,
      IAuthenticationApiClient
{
    public AuthenticationApiClient(
        HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<
            LoginRequest,
            LoginResponse>(
                "api/auth/login",
                request,
                cancellationToken);
    }

    public Task<UserResponse?> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<
            RegisterRequest,
            UserResponse>(
                "api/auth/register",
                request,
                cancellationToken);
    }

    public Task ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync(
            "api/auth/change-password",
            request,
            cancellationToken);
    }
}