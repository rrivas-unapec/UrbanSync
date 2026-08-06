using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Users;

public sealed class CreateUserRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(
        150,
        MinimumLength = 3,
        ErrorMessage = "El nombre completo debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(
        150,
        ErrorMessage = "El correo no puede exceder los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(
        8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Debe indicar un rol válido.")]
    public int RolId { get; set; }
}