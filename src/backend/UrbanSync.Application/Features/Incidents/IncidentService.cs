using UrbanSync.Application.Common.Interfaces.Persistence;
using UrbanSync.Application.Features.Audit;

namespace UrbanSync.Application.Features.Incidents;

public sealed class IncidentService : IIncidentService
{
    private const string AuditEntity = "Incidencias";

    private const string AuditActionCreate =
        "Reporte de incidencia";

    private const string AuditActionStatus =
        "Cambio de estado";

    private const string AuditActionTriage =
        "Triage";

    private const string AuditEmptyValue =
        "—";

    private const int AuditDetailMaxLength =
        400;

    private static readonly HashSet<string>
        AllowedPriorities =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Baja",
                "Media",
                "Alta",
                "Critica",
                "Crítica"
            };

    private static readonly HashSet<string>
        AllowedStatuses =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Registrada",
                "EnAnalisis",
                "Asignada",
                "EnProceso",
                "Cerrada",
                "Rechazada"
            };

    private readonly IIncidentRepository
        _incidentRepository;

    private readonly IAuditService
        _auditService;

    private readonly IIncidentNotificationService
        _incidentNotificationService;

    public IncidentService(
        IIncidentRepository incidentRepository,
        IAuditService auditService,
        IIncidentNotificationService incidentNotificationService)
    {
        _incidentRepository =
            incidentRepository;

        _auditService =
            auditService;

        _incidentNotificationService =
            incidentNotificationService;
    }

    public Task<IReadOnlyList<IncidentDto>> GetAllAsync(
        string? status = null,
        int? reportingUserId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedStatus =
            NormalizeOptionalValue(status);

        if (
            normalizedStatus is not null &&
            !AllowedStatuses.Contains(
                normalizedStatus))
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
        ArgumentNullException.ThrowIfNull(
            incident);

        if (reportingUserId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reportingUserId),
                "El ID del usuario debe ser mayor que cero.");
        }

        ValidateCreateIncident(
            incident);

        incident.Direccion =
            incident.Direccion.Trim();

        incident.Referencia =
            NormalizeOptionalValue(
                incident.Referencia);

        incident.Descripcion =
            incident.Descripcion.Trim();

        incident.Prioridad =
            NormalizePriority(
                incident.Prioridad);

        var caseCode =
            GenerateCaseCode();

        var incidentId =
            await _incidentRepository.CreateAsync(
                incident,
                reportingUserId,
                caseCode,
                cancellationToken);

        var createdIncident =
            await _incidentRepository.GetByIdAsync(
                incidentId,
                cancellationToken);

        if (createdIncident is null)
        {
            throw new InvalidOperationException(
                "La incidencia fue creada, pero no pudo recuperarse.");
        }

        await RegisterAuditAsync(
            reportingUserId,
            AuditActionCreate,
            createdIncident.Id,
            BuildDetail(
                $"Incidencia {createdIncident.CodigoCaso} registrada.",
                Change(
                    "Estado",
                    null,
                    createdIncident.Estado),
                Change(
                    "Prioridad",
                    null,
                    createdIncident.Prioridad)),
            cancellationToken);

        return createdIncident;
    }

    public async Task<IncidentDto?> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusDto incident,
        int actingUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            incident);

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

        if (
            !AllowedStatuses.Contains(
                normalizedStatus))
        {
            throw new ArgumentException(
                $"El estado '{incident.Estado}' no es válido.",
                nameof(incident));
        }

        var previous =
            await _incidentRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (previous is null)
        {
            return null;
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

        var current =
            await _incidentRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (current is not null)
        {
            await RegisterAuditAsync(
                actingUserId,
                AuditActionStatus,
                current.Id,
                BuildDetail(
                    $"Incidencia {current.CodigoCaso}.",
                    Change(
                        "Estado",
                        previous.Estado,
                        current.Estado),
                    Change(
                        "Institución asignada",
                        previous.InstitucionAsignada,
                        current.InstitucionAsignada)),
                cancellationToken);

            if (
                !string.Equals(
                    previous.Estado,
                    current.Estado,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _incidentNotificationService
                    .NotifyStatusChangedAsync(
                        previous,
                        current,
                        cancellationToken);
            }
        }

        return current;
    }

    public async Task<IncidentDto?> TriageAsync(
        int id,
        TriageIncidentDto incident,
        int actingUserId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            incident);

        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El ID de la incidencia debe ser mayor que cero.");
        }

        if (
            incident.TipoIncidenciaId
            is <= 0)
        {
            throw new ArgumentException(
                "El tipo de incidencia debe ser mayor que cero.",
                nameof(incident));
        }

        if (
            incident.JurisdiccionId
            is <= 0)
        {
            throw new ArgumentException(
                "La jurisdicción debe ser mayor que cero.",
                nameof(incident));
        }

        incident.Prioridad =
            NormalizeOptionalValue(
                incident.Prioridad);

        if (
            incident.Prioridad
            is not null)
        {
            incident.Prioridad =
                NormalizePriority(
                    incident.Prioridad);
        }

        incident.Accion =
            NormalizeOptionalValue(
                incident.Accion)?
                .ToLowerInvariant();

        var resultingStatus =
            ResolveTriageStatus(
                incident.Accion);

        var previous =
            await _incidentRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (previous is null)
        {
            return null;
        }

        var updated =
            await _incidentRepository.TriageAsync(
                id,
                incident,
                resultingStatus,
                cancellationToken);

        if (!updated)
        {
            return null;
        }

        var current =
            await _incidentRepository.GetByIdAsync(
                id,
                cancellationToken);

        if (current is not null)
        {
            await RegisterAuditAsync(
                actingUserId,
                AuditActionTriage,
                current.Id,
                BuildDetail(
                    $"Incidencia {current.CodigoCaso} analizada.",
                    Change(
                        "Estado",
                        previous.Estado,
                        current.Estado),
                    Change(
                        "Prioridad",
                        previous.Prioridad,
                        current.Prioridad),
                    Change(
                        "Tipo",
                        previous.TipoIncidencia,
                        current.TipoIncidencia),
                    Change(
                        "Jurisdicción",
                        previous.Jurisdiccion,
                        current.Jurisdiccion)),
                cancellationToken);

            if (
                !string.Equals(
                    previous.Estado,
                    current.Estado,
                    StringComparison.OrdinalIgnoreCase))
            {
                await _incidentNotificationService
                    .NotifyStatusChangedAsync(
                        previous,
                        current,
                        cancellationToken);
            }
        }

        return current;
    }

    private Task RegisterAuditAsync(
        int actingUserId,
        string action,
        int incidentId,
        string detail,
        CancellationToken cancellationToken)
    {
        return _auditService.CreateAsync(
            new CreateAuditDto
            {
                UserId =
                    actingUserId > 0
                        ? actingUserId
                        : null,

                Action =
                    action,

                Entity =
                    AuditEntity,

                EntityId =
                    incidentId,

                Detail =
                    detail
            },
            cancellationToken);
    }

    private static string? Change(
        string field,
        string? before,
        string? after)
    {
        var normalizedBefore =
            NormalizeOptionalValue(
                before);

        var normalizedAfter =
            NormalizeOptionalValue(
                after);

        if (
            string.Equals(
                normalizedBefore,
                normalizedAfter,
                StringComparison.Ordinal))
        {
            return null;
        }

        return
            $"{field}: " +
            $"{normalizedBefore ?? AuditEmptyValue} → " +
            $"{normalizedAfter ?? AuditEmptyValue}";
    }

    private static string BuildDetail(
        string summary,
        params string?[] changes)
    {
        var applied =
            changes
                .Where(
                    change =>
                        change is not null)
                .ToArray();

        var detail =
            applied.Length == 0
                ? summary
                : $"{summary} {string.Join("; ", applied)}";

        return
            detail.Length <= AuditDetailMaxLength
                ? detail
                : detail[..AuditDetailMaxLength];
    }

    private static void ValidateCreateIncident(
        CreateIncidentDto incident)
    {
        if (
            incident.TipoIncidenciaId
            <= 0)
        {
            throw new ArgumentException(
                "El tipo de incidencia es obligatorio.",
                nameof(incident));
        }

        if (
            incident.JurisdiccionId
            <= 0)
        {
            throw new ArgumentException(
                "La jurisdicción es obligatoria.",
                nameof(incident));
        }

        if (
            string.IsNullOrWhiteSpace(
                incident.Direccion))
        {
            throw new ArgumentException(
                "La dirección es obligatoria.",
                nameof(incident));
        }

        if (
            string.IsNullOrWhiteSpace(
                incident.Descripcion))
        {
            throw new ArgumentException(
                "La descripción es obligatoria.",
                nameof(incident));
        }

        if (
            incident.Direccion
                .Trim()
                .Length
            > 250)
        {
            throw new ArgumentException(
                "La dirección no puede superar 250 caracteres.",
                nameof(incident));
        }

        if (
            incident.Referencia?
                .Trim()
                .Length
            > 250)
        {
            throw new ArgumentException(
                "La referencia no puede superar 250 caracteres.",
                nameof(incident));
        }

        if (
            incident.Descripcion
                .Trim()
                .Length
            > 1000)
        {
            throw new ArgumentException(
                "La descripción no puede superar 1000 caracteres.",
                nameof(incident));
        }

        if (
            incident.Latitud
            is < -90 or > 90)
        {
            throw new ArgumentException(
                "La latitud debe estar entre -90 y 90.",
                nameof(incident));
        }

        if (
            incident.Longitud
            is < -180 or > 180)
        {
            throw new ArgumentException(
                "La longitud debe estar entre -180 y 180.",
                nameof(incident));
        }

        if (
            !AllowedPriorities.Contains(
                incident.Prioridad.Trim()))
        {
            throw new ArgumentException(
                $"La prioridad '{incident.Prioridad}' no es válida.",
                nameof(incident));
        }
    }

    private static string NormalizePriority(
        string priority)
    {
        var normalized =
            NormalizeRequiredValue(
                priority,
                nameof(priority));

        if (
            !AllowedPriorities.Contains(
                normalized))
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
            null =>
                null,

            "asignar" =>
                "Asignada",

            "aprobar" =>
                "Asignada",

            "rechazar" =>
                "Rechazada",

            _ =>
                throw new ArgumentException(
                    $"La acción de moderación '{action}' no es válida.",
                    nameof(action))
        };
    }

    private static string GenerateCaseCode()
    {
        var randomPart =
            Guid.NewGuid()
                .ToString("N")
                [..10]
                .ToUpperInvariant();

        return
            $"INC-{DateTime.UtcNow:yyyyMMdd}-{randomPart}";
    }

    private static string NormalizeRequiredValue(
        string value,
        string parameterName)
    {
        if (
            string.IsNullOrWhiteSpace(
                value))
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
        return
            string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
    }
}