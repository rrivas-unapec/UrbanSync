using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Institutions;

public sealed class InstitutionsApiClient
    : ApiClientBase,
      IInstitutionsApiClient
{
    public InstitutionsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<InstitutionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var institutions = await GetAsync<List<InstitutionResponse>>(
            "api/institutions",
            cancellationToken);

        return institutions ?? [];
    }

    public Task<InstitutionResponse?> CreateAsync(
        CreateInstitutionRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateInstitutionRequest, InstitutionResponse>(
            "api/institutions",
            request,
            cancellationToken);
    }
}
