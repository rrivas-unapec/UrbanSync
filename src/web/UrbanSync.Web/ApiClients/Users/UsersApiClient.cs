using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Users;

public sealed class UsersApiClient
    : ApiClientBase,
      IUsersApiClient
{
    public UsersApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await GetAsync<List<UserResponse>>(
            "api/usuarios",
            cancellationToken);

        return users ?? [];
    }

    public Task<UserResponse?> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateUserRequest, UserResponse>(
            "api/usuarios",
            request,
            cancellationToken);
    }

    public Task ToggleStatusAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync(
            $"api/usuarios/{id}/toggle-status",
            cancellationToken);
    }
}