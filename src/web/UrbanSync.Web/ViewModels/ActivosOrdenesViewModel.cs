namespace UrbanSync.Web.ViewModels;

public sealed class ActivosOrdenesViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<WorkOrderItemViewModel> Ordenes { get; set; } = [];
}

public sealed class WorkOrderItemViewModel
{
    public int Id { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string UsuarioAsignado { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Resultado { get; set; }
}
