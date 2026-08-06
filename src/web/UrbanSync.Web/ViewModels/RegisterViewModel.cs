using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Web.ViewModels;

public sealed class RegisterViewModel
{
    [Required(
        ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(
        150,
        MinimumLength = 3,
        ErrorMessage =
            "El nombre completo debe tener entre 3 y 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(
        ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [MinLength(
        6,
        ErrorMessage =
            "La contraseña debe tener al menos 6 caracteres.")]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "Debes confirmar la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}