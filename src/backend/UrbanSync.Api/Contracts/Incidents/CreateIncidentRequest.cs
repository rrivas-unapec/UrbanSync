using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Incidents;

public sealed class CreateIncidentRequest
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Debe indicar un tipo de incidencia válido.")]
    public int TipoIncidenciaId { get; set; }

    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(
        1000,
        MinimumLength = 10,
        ErrorMessage =
            "La descripción debe tener entre 10 y 1000 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La prioridad es obligatoria.")]
    [RegularExpression(
        "^(Baja|Media|Alta|Critica|Crítica)$",
        ErrorMessage =
            "La prioridad debe ser Baja, Media, Alta o Critica.")]
    public string Prioridad { get; set; } = "Media";

    [Required(ErrorMessage = "La ubicación es obligatoria.")]
    public IncidentLocationRequest Ubicacion { get; set; } = new();
}