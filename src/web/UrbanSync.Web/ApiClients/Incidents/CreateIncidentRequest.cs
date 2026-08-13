namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class CreateIncidentRequest
{
    public int TipoIncidenciaId { get; set; }

    public int? ActivoId { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Prioridad { get; set; } = "Media";

    public IncidentLocationRequest Ubicacion { get; set; } = new();
}
