using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Assets;

public sealed class CreateAssetRequest
{
    [Required(
        ErrorMessage =
            "El código del activo es obligatorio.")]
    [StringLength(
        50,
        ErrorMessage =
            "El código no puede superar 50 caracteres.")]
    public string Code { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "El nombre del activo es obligatorio.")]
    [StringLength(
        100,
        ErrorMessage =
            "El nombre no puede superar 100 caracteres.")]
    public string Name { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "El tipo del activo es obligatorio.")]
    [StringLength(
        50,
        ErrorMessage =
            "El tipo no puede superar 50 caracteres.")]
    public string Type { get; set; } =
        string.Empty;

    [Required(
        ErrorMessage =
            "El estado del activo es obligatorio.")]
    [StringLength(
        30,
        ErrorMessage =
            "El estado no puede superar 30 caracteres.")]
    public string Status { get; set; } =
        "Operativo";

    [Range(
        1,
        int.MaxValue,
        ErrorMessage =
            "Debe indicar una jurisdicción válida.")]
    public int JurisdictionId { get; set; }

    public DateTime? InstallationDate { get; set; }
}