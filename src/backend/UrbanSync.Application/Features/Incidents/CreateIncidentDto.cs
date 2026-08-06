namespace UrbanSync.Application.Features.Incidents;

public sealed class CreateIncidentDto
{
    public int TipoIncidenciaId { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int JurisdiccionId { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public string Prioridad { get; set; } = "Media";
}