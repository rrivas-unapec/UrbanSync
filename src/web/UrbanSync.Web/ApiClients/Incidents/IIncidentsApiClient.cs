namespace UrbanSync.Web.ApiClients.Incidents;

public interface IIncidentsApiClient
{
    Task<IReadOnlyList<IncidentResponse>> GetAllAsync(
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<IncidentResponse?> TriageAsync(
        int id,
        TriageIncidentRequest request,
        CancellationToken cancellationToken = default);
}