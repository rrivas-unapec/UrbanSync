namespace UrbanSync.Web.ViewModels;

public sealed class MapaViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<MapaTipoViewModel> Tipos { get; set; } = [];
    public List<IncidentMapPointViewModel> Puntos { get; set; } = [];
}

public sealed class MapaTipoViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public sealed class IncidentMapPointViewModel
{
    public int Id { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string TipoIncidencia { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
