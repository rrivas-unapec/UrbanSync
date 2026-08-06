
namespace UrbanSync.Domain.DTOs
{
    public class AuditoriaCreateDto
    {
        public int? UsuarioId { get; set; }
        public string Accion { get; set; } = string.Empty;
        public string? Entidad { get; set; }
        public int? EntidadId { get; set; }
        public string? Detalle { get; set; }
        public string? IpOrigen { get; set; }
    }

    public class AuditoriaDto
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

    public class AuditoriaFilterDto
    {
        public int? UsuarioId { get; set; }
        public string? Entidad { get; set; }
        public string? Accion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
