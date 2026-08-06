namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class IncidentResponse
{
    public int Id { get; set; }

    public string CodigoCaso { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string Prioridad { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public int TipoIncidenciaId { get; set; }

    public string TipoIncidencia { get; set; } = string.Empty;

    public int JurisdiccionId { get; set; }

    public string Jurisdiccion { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string UsuarioReporta { get; set; } = string.Empty;

    public DateTime FechaReporte { get; set; }

    public int? InstitucionAsignadaId { get; set; }

    public string? InstitucionAsignada { get; set; }

    public string? Referencia { get; set; }

    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    public DateTime? FechaAsignacion { get; set; }

    public DateTime? FechaCierre { get; set; }
}