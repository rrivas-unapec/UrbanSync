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

public sealed class ApiReportCountDto
{
    public string Clave { get; set; } = string.Empty;
    public int Total { get; set; }
}

public sealed class ApiReportSummaryDto
{
    public int Total { get; set; }
    public List<ApiReportCountDto> PorEstado { get; set; } = [];
    public List<ApiReportCountDto> PorTipo { get; set; } = [];
    public List<ApiReportCountDto> PorPrioridad { get; set; } = [];
    public List<ApiReportCountDto> PorJurisdiccion { get; set; } = [];
}

public sealed class ApiIncidentDto
{
    public int Id { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int TipoIncidenciaId { get; set; }
    public string TipoIncidencia { get; set; } = string.Empty;
    public int JurisdiccionId { get; set; }
    public string Jurisdiccion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string UsuarioReporta { get; set; } = string.Empty;
    public DateTime FechaReporte { get; set; }
    public int? InstitucionAsignadaId { get; set; }
    public string? InstitucionAsignada { get; set; }
    public string? Referencia { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public DateTime? FechaAsignacion { get; set; }
    public DateTime? FechaCierre { get; set; }
}

public sealed class ApiIncidentTriageRequest
{
    public int? TipoIncidenciaId { get; set; }
    public string? Prioridad { get; set; }
    public string? Accion { get; set; }
    public int? JurisdiccionId { get; set; }
}

public sealed class ApiWorkOrderDto
{
    public int Id { get; set; }
    public int IncidenciaId { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string UsuarioAsignadoId { get; set; } = string.Empty;
    public string UsuarioAsignado { get; set; } = string.Empty;
    public string DescripcionTrabajo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string? Resultado { get; set; }
}
