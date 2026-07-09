
namespace UrbanSync.Domain.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int RolId { get; set; }
        public bool Activo { get; set; }
    }

    public class UsuarioCreateDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RolId { get; set; }
    }
}
