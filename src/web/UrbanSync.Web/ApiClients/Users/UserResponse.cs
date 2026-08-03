namespace UrbanSync.Web.ApiClients.Users;

public sealed class UserResponse
{
    public int Id { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int RolId { get; set; }

    public string RolNombre { get; set; } = string.Empty;

    public bool Activo { get; set; }
}