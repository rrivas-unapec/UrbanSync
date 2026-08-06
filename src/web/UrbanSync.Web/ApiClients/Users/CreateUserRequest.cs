namespace UrbanSync.Web.ApiClients.Users;

public sealed class CreateUserRequest
{
    public string NombreUsuario { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int RolId { get; set; }
}