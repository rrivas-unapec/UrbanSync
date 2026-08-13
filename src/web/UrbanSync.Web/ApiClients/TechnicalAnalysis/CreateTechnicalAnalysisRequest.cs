namespace UrbanSync.Web.ApiClients.TechnicalAnalysis;

public sealed class CreateTechnicalAnalysisRequest
{
    public int IncidentId { get; set; }

    public int TechnicalUserId { get; set; }

    public string Diagnosis { get; set; } = string.Empty;

    public string? RecommendedActions { get; set; }
}
