namespace UrbanSync.Web.ApiClients.Roles;

public interface IRolesApiClient
{
    Task<IReadOnlyList<RoleResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}