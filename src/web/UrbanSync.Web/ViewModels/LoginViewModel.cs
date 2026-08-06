using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Web.ViewModels;

public sealed class LoginViewModel
{
    [Required(
        ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(
        ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada")]
    public bool RememberMe { get; set; }
}