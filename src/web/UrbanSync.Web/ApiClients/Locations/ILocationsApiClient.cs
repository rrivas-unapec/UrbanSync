namespace UrbanSync.Web.ApiClients.Locations;

public interface ILocationsApiClient
{
    Task<IReadOnlyList<LocationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<LocationResponse?> CreateAsync(
        CreateLocationRequest request,
        CancellationToken cancellationToken = default);
}
