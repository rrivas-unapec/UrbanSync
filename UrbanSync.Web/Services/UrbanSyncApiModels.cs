namespace UrbanSync.Web.Services;

public sealed class ApiRoleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public sealed class ApiUserDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public sealed class ApiCreateUserRequest
{
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RolId { get; set; }
}

public sealed class ApiLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class ApiLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public ApiUserDto User { get; set; } = new();
}

public sealed class ApiErrorResponse
{
    public string? Mensaje { get; set; }
    public string? Message { get; set; }
    public string? Title { get; set; }
    public string? Detail { get; set; }
}
