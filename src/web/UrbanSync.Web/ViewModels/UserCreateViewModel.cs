using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Web.ViewModels;

public sealed class UserCreateViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(
        150,
        MinimumLength = 3,
        ErrorMessage =
            "El nombre completo debe tener entre 3 y 150 caracteres.")]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo inválido.")]
    [StringLength(
        150,
        ErrorMessage = "El correo no puede exceder 150 caracteres.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Debe seleccionar un rol.")]
    [Display(Name = "Rol")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(
        8,
        ErrorMessage =
            "La contraseña debe tener al menos 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña temporal")]
    public string Password { get; set; } = string.Empty;
}