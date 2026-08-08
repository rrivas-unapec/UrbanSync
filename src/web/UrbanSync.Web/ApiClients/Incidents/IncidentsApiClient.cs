using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class IncidentsApiClient
    : ApiClientBase,
      IIncidentsApiClient
{
    public IncidentsApiClient(
        HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<IncidentResponse>> GetAllAsync(
        string? status = null,
        bool mine = false,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add(
                $"status={Uri.EscapeDataString(status)}");
        }

        if (mine)
        {
            query.Add("mine=true");
        }

        var uri = "api/incidents";

        if (query.Count > 0)
        {
            uri += $"?{string.Join("&", query)}";
        }

        var incidents =
            await GetAsync<List<IncidentResponse>>(
                uri,
                cancellationToken);

        return incidents ?? [];
    }

    public Task<IncidentResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<IncidentResponse>(
            $"api/incidents/{id}",
            cancellationToken);
    }

    public Task<IncidentResponse?> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync<
            UpdateIncidentStatusRequest,
            IncidentResponse>(
                $"api/incidents/{id}/status",
                request,
                cancellationToken);
    }

    public Task<IncidentResponse?> TriageAsync(
        int id,
        TriageIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PatchAsync<
            TriageIncidentRequest,
            IncidentResponse>(
                $"api/incidents/{id}/triage",
                request,
                cancellationToken);
    }
}