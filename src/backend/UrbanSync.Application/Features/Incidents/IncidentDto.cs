namespace UrbanSync.Application.Features.Incidents;

public sealed class IncidentDto
{
    public int Id { get; set; }

    public string CodigoCaso { get; set; } =
        string.Empty;

    public int UsuarioReportaId { get; set; }

    public string UsuarioReporta { get; set; } =
        string.Empty;

    public int TipoIncidenciaId { get; set; }

    public string TipoIncidencia { get; set; } =
        string.Empty;

    public int? ActivoId { get; set; }

    public int UbicacionId { get; set; }

    public string Direccion { get; set; } =
        string.Empty;

    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public int JurisdiccionId { get; set; }

    public string Jurisdiccion { get; set; } =
        string.Empty;

    public int? InstitucionAsignadaId { get; set; }

    public string? InstitucionAsignada { get; set; }

    public string Estado { get; set; } =
        string.Empty;

    public string Prioridad { get; set; } =
        string.Empty;

    public string Descripcion { get; set; } =
        string.Empty;

    public DateTime FechaReporte { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaCierre { get; set; }
}