namespace UrbanSync.Web.ApiClients.Authentication;

public interface IAuthenticationApiClient
{
    Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
}