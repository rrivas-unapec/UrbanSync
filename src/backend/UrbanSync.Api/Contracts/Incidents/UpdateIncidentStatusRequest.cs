using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Incidents;

public sealed class UpdateIncidentStatusRequest
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression(
        "^(Registrada|EnAnalisis|Asignada|EnProceso|Cerrada|Rechazada)$",
        ErrorMessage =
            "El estado proporcionado no es válido.")]
    public string Estado { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "La institución asignada debe tener un ID válido.")]
    public int? InstitucionAsignadaId { get; set; }
}