namespace UrbanSync.Api.Contracts.Roles;

public sealed class RoleResponse
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }
}