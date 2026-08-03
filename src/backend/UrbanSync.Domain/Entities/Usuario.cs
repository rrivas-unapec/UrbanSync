namespace UrbanSync.Domain.Entities;

public sealed class Usuario
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public byte[] PasswordHash { get; set; } = [];

    public byte[] PasswordSalt { get; set; } = [];

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }
}