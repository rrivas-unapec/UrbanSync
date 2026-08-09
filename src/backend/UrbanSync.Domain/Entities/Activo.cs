
namespace UrbanSync.Domain.Entities
{
    public class Activo
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Estado { get; set; } = "Operativo";
        public int JurisdiccionId { get; set; }
        public string? NombreJurisdiccion { get; set; }
        public DateTime? FechaInstalacion { get; set; }
        public bool ActivoEstado { get; set; } = true;
    }
}
