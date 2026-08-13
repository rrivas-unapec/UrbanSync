namespace UrbanSync.Web.ApiClients.TechnicalAnalysis;

public sealed class TechnicalAnalysisResponse
{
    public int Id { get; set; }

    public int IncidentId { get; set; }

    public int TechnicalUserId { get; set; }

    public string TechnicalUserName { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public string? RecommendedActions { get; set; }

    public DateTime AnalysisDate { get; set; }
}
