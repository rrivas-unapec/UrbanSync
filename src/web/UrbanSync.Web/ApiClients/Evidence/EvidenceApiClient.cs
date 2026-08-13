using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Evidence;

public sealed class EvidenceApiClient
    : ApiClientBase,
      IEvidenceApiClient
{
    public EvidenceApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<EvidenceResponse>> GetByIncidentIdAsync(
        int incidentId,
        CancellationToken cancellationToken = default)
    {
        var evidences = await GetAsync<List<EvidenceResponse>>(
            $"api/evidences/by-incident/{incidentId}",
            cancellationToken);

        return evidences ?? [];
    }

    public Task<EvidenceResponse?> CreateAsync(
        CreateEvidenceRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateEvidenceRequest, EvidenceResponse>(
            "api/evidences",
            request,
            cancellationToken);
    }
}
