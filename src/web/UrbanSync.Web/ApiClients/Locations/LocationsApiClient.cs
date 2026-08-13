using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Locations;

public sealed class LocationsApiClient
    : ApiClientBase,
      ILocationsApiClient
{
    public LocationsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<LocationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var locations = await GetAsync<List<LocationResponse>>(
            "api/locations",
            cancellationToken);

        return locations ?? [];
    }

    public Task<LocationResponse?> CreateAsync(
        CreateLocationRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateLocationRequest, LocationResponse>(
            "api/locations",
            request,
            cancellationToken);
    }
}
