namespace UrbanSync.Web.ApiClients.TechnicalAnalysis;

public interface ITechnicalAnalysisApiClient
{
    Task<TechnicalAnalysisResponse?> GetByIncidentIdAsync(
        int incidentId,
        CancellationToken cancellationToken = default);

    Task<TechnicalAnalysisResponse?> CreateAsync(
        CreateTechnicalAnalysisRequest request,
        CancellationToken cancellationToken = default);
}
