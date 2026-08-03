using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Authentication;

public sealed class AuthenticationApiClient
    : ApiClientBase,
      IAuthenticationApiClient
{
    public AuthenticationApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<LoginRequest, LoginResponse>(
            "api/auth/login",
            request,
            cancellationToken);
    }
}