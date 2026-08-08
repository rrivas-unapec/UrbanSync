namespace UrbanSync.Web.ApiClients.Activity;

public sealed class ActivityResponse
{
    public long Id { get; set; }

    public int? UsuarioId { get; set; }

    public string? NombreUsuario { get; set; }

    public string Accion { get; set; } = string.Empty;

    public string? Entidad { get; set; }

    public int? EntidadId { get; set; }

    public string? Detalle { get; set; }

    public string? IpOrigen { get; set; }

    public DateTime FechaHora { get; set; }
}