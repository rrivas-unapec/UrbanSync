using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class IncidentsApiClient
    : ApiClientBase,
      IIncidentsApiClient
{
    public IncidentsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<IncidentResponse>> GetAllAsync(
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var uri = "api/incidents";

        if (!string.IsNullOrWhiteSpace(status))
        {
            uri += $"?status={Uri.EscapeDataString(status)}";
        }

        var incidents = await GetAsync<List<IncidentResponse>>(
            uri,
            cancellationToken);

        return incidents ?? [];
    }

    public Task<IncidentResponse?> TriageAsync(
        int id,
        TriageIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync<TriageIncidentRequest, IncidentResponse>(
            $"api/incidents/{id}/triage",
            request,
            cancellationToken);
    }
}