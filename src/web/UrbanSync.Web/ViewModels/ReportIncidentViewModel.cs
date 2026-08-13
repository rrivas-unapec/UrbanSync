namespace UrbanSync.Web.ViewModels;

public sealed class ReportIncidentPageViewModel
{
    public List<IncidentTypeOptionViewModel> TiposIncidencia { get; set; } = [];
    public List<LocationOptionViewModel> Ubicaciones { get; set; } = [];
}

public sealed class IncidentTypeOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public sealed class LocationOptionViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string JurisdictionName { get; set; } = string.Empty;
}
