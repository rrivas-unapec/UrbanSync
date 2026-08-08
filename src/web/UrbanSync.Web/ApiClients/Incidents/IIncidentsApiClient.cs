namespace UrbanSync.Web.ApiClients.Incidents;

public interface IIncidentsApiClient
{
    Task<IReadOnlyList<IncidentResponse>> GetAllAsync(
        string? status = null,
        bool mine = false,
        CancellationToken cancellationToken = default);

    Task<IncidentResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IncidentResponse?> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<IncidentResponse?> TriageAsync(
        int id,
        TriageIncidentRequest request,
        CancellationToken cancellationToken = default);
}