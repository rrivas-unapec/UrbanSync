using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Web.ViewModels;

public sealed class ChangePasswordViewModel
{
    [Required(
        ErrorMessage =
            "La contraseña actual es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string CurrentPassword { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "La nueva contraseña es obligatoria.")]
    [MinLength(
        8,
        ErrorMessage =
            "La nueva contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "Debes confirmar la nueva contraseña.")]
    [Compare(
        nameof(NewPassword),
        ErrorMessage =
            "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar nueva contraseña")]
    public string ConfirmNewPassword { get; set; } =
        string.Empty;
}