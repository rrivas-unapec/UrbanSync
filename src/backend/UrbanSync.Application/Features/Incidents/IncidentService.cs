using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Incidents;

public sealed class IncidentService : IIncidentService
{
    private static readonly HashSet<string> AllowedPriorities =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Baja",
            "Media",
            "Alta",
            "Critica",
            "Crítica"
        };

    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Registrada",
            "EnAnalisis",
            "Asignada",
            "EnProceso",
            "Cerrada",
            "Rechazada"
        };

    private readonly IIncidentRepository _incidentRepository;

    public IncidentService(
        IIncidentRepository incidentRepository)
    {
        _incidentRepository = incidentRepository;
    }

    public Task<IReadOnlyList<IncidentDto>> GetAllAsync(
        string? status = null,
        int? reportingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus = NormalizeOptionalValue(status);

        if (normalizedStatus is not null &&
            !AllowedStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentException(
                $"El estado '{status}' no es válido.",
                nameof(status));
        }

        return _incidentRepository.GetAllAsync(
            normalizedStatus,
            reportingUserId,
            cancellationToken);
    }

    public Task<IncidentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El ID de la incidencia debe ser mayor que cero.");
        }

        return _incidentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<IncidentDto> CreateAsync(
        CreateIncidentDto incident,
        int reportingUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        if (reportingUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reportingUserId),
                "El ID del usuario debe ser mayor que cero.");
        }

        ValidateCreateIncident(incident);

        incident.Direccion = incident.Direccion.Trim();
        incident.Referencia =
            NormalizeOptionalValue(incident.Referencia);
        incident.Descripcion = incident.Descripcion.Trim();
        incident.Prioridad = NormalizePriority(incident.Prioridad);

        var caseCode = GenerateCaseCode();

        var incidentId = await _incidentRepository.CreateAsync(
            incident,
            reportingUserId,
            caseCode,
            cancellationToken);

        var createdIncident =
            await _incidentRepository.GetByIdAsync(
                incidentId,
                cancellationToken);

        return createdIncident
            ?? throw new InvalidOperationException(
                "La incidencia fue creada, pero no pudo recuperarse.");
    }

    public async Task<IncidentDto?> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusDto incident,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El ID de la incidencia debe ser mayor que cero.");
        }

        var normalizedStatus =
            NormalizeRequiredValue(
                incident.Estado,
                nameof(incident.Estado));

        if (!AllowedStatuses.Contains(normalizedStatus))
        {
            throw new ArgumentException(
                $"El estado '{incident.Estado}' no es válido.",
                nameof(incident));
        }

        var updated =
            await _incidentRepository.UpdateStatusAsync(
                id,
                normalizedStatus,
                incident.InstitucionAsignadaId,
                cancellationToken);

        if (!updated)
        {
            return null;
        }

        return await _incidentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<IncidentDto?> TriageAsync(
        int id,
        TriageIncidentDto incident,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El ID de la incidencia debe ser mayor que cero.");
        }

        if (incident.TipoIncidenciaId is <= 0)
        {
            throw new ArgumentException(
                "El tipo de incidencia debe ser mayor que cero.",
                nameof(incident));
        }

        if (incident.JurisdiccionId is <= 0)
        {
            throw new ArgumentException(
                "La jurisdicción debe ser mayor que cero.",
                nameof(incident));
        }

        incident.Prioridad =
            NormalizeOptionalValue(incident.Prioridad);

        if (incident.Prioridad is not null)
        {
            incident.Prioridad =
                NormalizePriority(incident.Prioridad);
        }

        incident.Accion =
            NormalizeOptionalValue(incident.Accion)?
                .ToLowerInvariant();

        var resultingStatus = ResolveTriageStatus(
            incident.Accion);

        var updated = await _incidentRepository.TriageAsync(
            id,
            incident,
            resultingStatus,
            cancellationToken);

        if (!updated)
        {
            return null;
        }

        return await _incidentRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    private static void ValidateCreateIncident(
        CreateIncidentDto incident)
    {
        if (incident.TipoIncidenciaId <= 0)
        {
            throw new ArgumentException(
                "El tipo de incidencia es obligatorio.",
                nameof(incident));
        }

        if (incident.JurisdiccionId <= 0)
        {
            throw new ArgumentException(
                "La jurisdicción es obligatoria.",
                nameof(incident));
        }

        if (string.IsNullOrWhiteSpace(incident.Direccion))
        {
            throw new ArgumentException(
                "La dirección es obligatoria.",
                nameof(incident));
        }

        if (string.IsNullOrWhiteSpace(incident.Descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria.",
                nameof(incident));
        }

        if (incident.Direccion.Trim().Length > 250)
        {
            throw new ArgumentException(
                "La dirección no puede superar 250 caracteres.",
                nameof(incident));
        }

        if (incident.Referencia?.Trim().Length > 250)
        {
            throw new ArgumentException(
                "La referencia no puede superar 250 caracteres.",
                nameof(incident));
        }

        if (incident.Descripcion.Trim().Length > 1000)
        {
            throw new ArgumentException(
                "La descripción no puede superar 1000 caracteres.",
                nameof(incident));
        }

        if (incident.Latitud is < -90 or > 90)
        {
            throw new ArgumentException(
                "La latitud debe estar entre -90 y 90.",
                nameof(incident));
        }

        if (incident.Longitud is < -180 or > 180)
        {
            throw new ArgumentException(
                "La longitud debe estar entre -180 y 180.",
                nameof(incident));
        }

        if (!AllowedPriorities.Contains(
                incident.Prioridad.Trim()))
        {
            throw new ArgumentException(
                $"La prioridad '{incident.Prioridad}' no es válida.",
                nameof(incident));
        }
    }

    private static string NormalizePriority(string priority)
    {
        var normalized =
            NormalizeRequiredValue(
                priority,
                nameof(priority));

        if (!AllowedPriorities.Contains(normalized))
        {
            throw new ArgumentException(
                $"La prioridad '{priority}' no es válida.",
                nameof(priority));
        }

        return normalized.Equals(
            "Crítica",
            StringComparison.OrdinalIgnoreCase)
            ? "Critica"
            : normalized;
    }

    private static string? ResolveTriageStatus(
        string? action)
    {
        return action switch
        {
            null => null,
            "asignar" => "Asignada",
            "aprobar" => "Asignada",
            "rechazar" => "Rechazada",
            _ => throw new ArgumentException(
                $"La acción de moderación '{action}' no es válida.",
                nameof(action))
        };
    }

    private static string GenerateCaseCode()
    {
        var randomPart = Guid.NewGuid()
            .ToString("N")
            [..10]
            .ToUpperInvariant();

        return $"INC-{DateTime.UtcNow:yyyyMMdd}-{randomPart}";
    }

    private static string NormalizeRequiredValue(
        string value,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "El valor es obligatorio.",
                parameterName);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}