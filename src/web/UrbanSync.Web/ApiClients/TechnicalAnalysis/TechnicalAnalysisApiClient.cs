using System.Net;
using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.TechnicalAnalysis;

public sealed class TechnicalAnalysisApiClient
    : ApiClientBase,
      ITechnicalAnalysisApiClient
{
    public TechnicalAnalysisApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<TechnicalAnalysisResponse?> GetByIncidentIdAsync(
        int incidentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<TechnicalAnalysisResponse>(
                $"api/technical-analyses/by-incident/{incidentId}",
                cancellationToken);
        }
        catch (UrbanSyncApiException exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task<TechnicalAnalysisResponse?> CreateAsync(
        CreateTechnicalAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateTechnicalAnalysisRequest, TechnicalAnalysisResponse>(
            "api/technical-analyses",
            request,
            cancellationToken);
    }
}
