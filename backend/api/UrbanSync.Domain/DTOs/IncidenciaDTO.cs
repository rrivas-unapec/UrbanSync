using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Domain.DTOs
{
    public class IncidenciaCreateDto
    {
        public int UsuarioReportaId { get; set; }
        public int TipoIncidenciaId { get; set; }
        public string Direccion { get; set; } = string.Empty;
        public string? Referencia { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int JurisdiccionId { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Prioridad { get; set; } = "Media";
    }

    public class IncidenciaDto
    {
        public int Id { get; set; }
        public string CodigoCaso { get; set; } = string.Empty;
        public int UsuarioReportaId { get; set; }
        public string NombreUsuarioReporta { get; set; } = string.Empty;
        public int TipoIncidenciaId { get; set; }
        public string NombreTipoIncidencia { get; set; } = string.Empty;
        public int UbicacionId { get; set; }
        public string DireccionUbicacion { get; set; } = string.Empty;
        public int? InstitucionAsignadaId { get; set; }
        public string? NombreInstitucionAsignada { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Prioridad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime FechaReporte { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaCierre { get; set; }
    }

    public class IncidenciaEstadoUpdateDto
    {
        public string Estado { get; set; } = string.Empty;
        public int? InstitucionAsignadaId { get; set; }
    }
}
