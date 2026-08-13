namespace UrbanSync.Web.ViewModels;

public sealed class AssetsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<AssetItemViewModel> Activos { get; set; } = [];
    public List<JurisdictionOptionViewModel> OpcionesJurisdiccion { get; set; } = [];
}

public sealed class AssetItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string JurisdictionName { get; set; } = string.Empty;
    public DateTime? InstallationDate { get; set; }
    public bool IsActive { get; set; }
}

public sealed class AssetDetailsViewModel
{
    public required AssetItemViewModel Asset { get; init; }

    public bool HistorialDisponible { get; set; } = true;
    public List<AssetHistoryItemViewModel> Historial { get; set; } = [];
}

public sealed class AssetHistoryItemViewModel
{
    public int IncidentId { get; set; }
    public string CaseCode { get; set; } = string.Empty;
    public string IncidentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
