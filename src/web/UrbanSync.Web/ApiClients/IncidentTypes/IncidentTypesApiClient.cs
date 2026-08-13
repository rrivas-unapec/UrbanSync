using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.IncidentTypes;

public sealed class IncidentTypesApiClient
    : ApiClientBase,
      IIncidentTypesApiClient
{
    public IncidentTypesApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<IncidentTypeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var types = await GetAsync<List<IncidentTypeResponse>>(
            "api/incident-types",
            cancellationToken);

        return types ?? [];
    }
}
