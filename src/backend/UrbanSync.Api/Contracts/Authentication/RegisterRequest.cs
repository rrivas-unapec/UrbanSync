using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Authentication;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(
        150,
        MinimumLength = 3,
        ErrorMessage =
            "El nombre completo debe tener entre 3 y 150 caracteres.")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [StringLength(
        150,
        ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(
        8,
        ErrorMessage =
            "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}