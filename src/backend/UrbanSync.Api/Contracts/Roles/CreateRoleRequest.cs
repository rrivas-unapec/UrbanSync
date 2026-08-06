using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Roles;

public sealed class CreateRoleRequest
{
    [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "El nombre del rol debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(
        500,
        ErrorMessage = "La descripción no puede exceder los 500 caracteres.")]
    public string? Descripcion { get; set; }
}