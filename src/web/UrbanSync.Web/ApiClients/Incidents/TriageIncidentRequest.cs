namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class TriageIncidentRequest
{
    public int? TipoIncidenciaId { get; set; }

    public string? Prioridad { get; set; }

    public string? Accion { get; set; }

    public int? JurisdiccionId { get; set; }
}