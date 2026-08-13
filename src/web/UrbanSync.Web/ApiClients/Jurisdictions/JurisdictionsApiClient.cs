using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Jurisdictions;

public sealed class JurisdictionsApiClient
    : ApiClientBase,
      IJurisdictionsApiClient
{
    public JurisdictionsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<JurisdictionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var jurisdictions = await GetAsync<List<JurisdictionResponse>>(
            "api/jurisdictions",
            cancellationToken);

        return jurisdictions ?? [];
    }

    public Task<JurisdictionResponse?> CreateAsync(
        CreateJurisdictionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateJurisdictionRequest, JurisdictionResponse>(
            "api/jurisdictions",
            request,
            cancellationToken);
    }
}
