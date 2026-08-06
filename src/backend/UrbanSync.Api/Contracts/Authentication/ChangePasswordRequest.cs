using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Authentication;

public sealed class ChangePasswordRequest
{
    [Required(
        ErrorMessage = "La contraseña actual es obligatoria.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(
        8,
        ErrorMessage =
            "La nueva contraseña debe tener al menos 8 caracteres.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "Debes confirmar la nueva contraseña.")]
    [Compare(
        nameof(NewPassword),
        ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}