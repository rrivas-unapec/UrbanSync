using System.ComponentModel.DataAnnotations;

namespace UrbanSync.Api.Contracts.Incidents;

public sealed class IncidentLocationRequest
{
    [Range(
        -90,
        90,
        ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public decimal? Lat { get; set; }

    [Range(
        -180,
        180,
        ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public decimal? Lng { get; set; }

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(
        250,
        MinimumLength = 3,
        ErrorMessage =
            "La dirección debe tener entre 3 y 250 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    [StringLength(
        250,
        ErrorMessage =
            "La referencia no puede superar 250 caracteres.")]
    public string? Referencia { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Debe indicar una jurisdicción válida.")]
    public int? JurisdiccionId { get; set; }
}