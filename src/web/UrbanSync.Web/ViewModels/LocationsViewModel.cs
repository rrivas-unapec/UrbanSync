namespace UrbanSync.Web.ViewModels;

public sealed class LocationsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<LocationItemViewModel> Ubicaciones { get; set; } = [];
    public List<JurisdictionOptionViewModel> OpcionesJurisdiccion { get; set; } = [];
}

public sealed class LocationItemViewModel
{
    public int Id { get; set; }
    public string Address { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string JurisdictionName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
