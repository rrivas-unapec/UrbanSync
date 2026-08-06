using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Roles;

public sealed class RolesApiClient
    : ApiClientBase,
      IRolesApiClient
{
    public RolesApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<RoleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await GetAsync<List<RoleResponse>>(
            "api/roles",
            cancellationToken);

        return roles ?? [];
    }
}