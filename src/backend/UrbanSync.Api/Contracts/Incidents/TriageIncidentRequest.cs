using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Incidents;

public sealed class TriageIncidentRequest
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "El tipo de incidencia debe tener un ID válido.")]
    public int? TipoIncidenciaId { get; set; }

    [RegularExpression(
        "^(Baja|Media|Alta|Critica|Crítica)$",
        ErrorMessage =
            "La prioridad debe ser Baja, Media, Alta o Critica.")]
    public string? Prioridad { get; set; }

    [RegularExpression(
        "^(asignar|aprobar|rechazar)$",
        ErrorMessage =
            "La acción debe ser asignar, aprobar o rechazar.")]
    public string? Accion { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "La jurisdicción debe tener un ID válido.")]
    public int? JurisdiccionId { get; set; }
}