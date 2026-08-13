namespace UrbanSync.Web.ViewModels;

public sealed class ModeracionViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<IncidentQueueItemViewModel> Cola { get; set; } = [];
    public List<IncidentTypeOptionViewModel> TiposIncidencia { get; set; } = [];
}

public sealed class IncidentQueueItemViewModel
{
    public int Id { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string TipoIncidencia { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Jurisdiccion { get; set; } = string.Empty;
    public string UsuarioReporta { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
}
