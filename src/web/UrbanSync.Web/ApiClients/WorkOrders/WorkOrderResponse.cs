namespace UrbanSync.Web.ApiClients.WorkOrders;

public sealed class WorkOrderResponse
{
    public int Id { get; set; }

    public int IncidenciaId { get; set; }

    public string CodigoCaso { get; set; } = string.Empty;

    public string UsuarioAsignadoId { get; set; } = string.Empty;

    public string UsuarioAsignado { get; set; } = string.Empty;

    public string DescripcionTrabajo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Resultado { get; set; }
}