using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Audit;

public sealed class CreateAuditRequest
{
    [Required(
        ErrorMessage = "La acción es obligatoria.")]
    [StringLength(
        50,
        MinimumLength = 2,
        ErrorMessage =
            "La acción debe tener entre 2 y 50 caracteres.")]
    public string Accion { get; set; } = string.Empty;

    [StringLength(
        80,
        ErrorMessage =
            "La entidad no puede superar 80 caracteres.")]
    public string? Entidad { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "El identificador de la entidad debe ser mayor que cero.")]
    public int? EntidadId { get; set; }

    [StringLength(
        400,
        ErrorMessage =
            "El detalle no puede superar 400 caracteres.")]
    public string? Detalle { get; set; }
}