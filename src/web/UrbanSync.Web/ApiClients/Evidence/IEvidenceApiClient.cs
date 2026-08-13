namespace UrbanSync.Web.ApiClients.Evidence;

public interface IEvidenceApiClient
{
    Task<IReadOnlyList<EvidenceResponse>> GetByIncidentIdAsync(
        int incidentId,
        CancellationToken cancellationToken = default);

    Task<EvidenceResponse?> CreateAsync(
        CreateEvidenceRequest request,
        CancellationToken cancellationToken = default);
}
