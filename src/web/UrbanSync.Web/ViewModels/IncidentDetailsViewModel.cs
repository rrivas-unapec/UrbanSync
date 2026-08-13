using UrbanSync.Web.ApiClients.Incidents;

namespace UrbanSync.Web.ViewModels;

public sealed class IncidentDetailsViewModel
{
    public required IncidentResponse Incident { get; init; }

    public bool EvidenciasDisponibles { get; set; } = true;
    public List<EvidenceItemViewModel> Evidencias { get; set; } = [];

    public bool AnalisisTecnicoDisponible { get; set; } = true;
    public TechnicalAnalysisItemViewModel? AnalisisTecnico { get; set; }
}

public sealed class EvidenceItemViewModel
{
    public int Id { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UploadedByUserName { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public sealed class TechnicalAnalysisItemViewModel
{
    public int Id { get; set; }
    public string TechnicalUserName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string? RecommendedActions { get; set; }
    public DateTime AnalysisDate { get; set; }
}
