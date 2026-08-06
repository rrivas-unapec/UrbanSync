namespace UrbanSync.Application.Features.Incidents;

public sealed class TriageIncidentDto
{
    public int? TipoIncidenciaId { get; set; }

    public string? Prioridad { get; set; }

    public string? Accion { get; set; }

    public int? JurisdiccionId { get; set; }
}