using UrbanSync.Application.Common.Interfaces.Persistence;

namespace UrbanSync.Application.Features.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public Task<IReadOnlyList<AuditDto>> GetAllAsync(
        AuditFilterDto? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (filter is null)
        {
            return _auditRepository.GetAllAsync(
                cancellationToken: cancellationToken);
        }

        if (filter.UserId.HasValue &&
            filter.UserId.Value <= 0)
        {
            throw new ArgumentException(
                "El identificador del usuario debe ser mayor que cero.",
                nameof(filter));
        }

        if (filter.StartDate.HasValue &&
            filter.EndDate.HasValue &&
            filter.StartDate.Value > filter.EndDate.Value)
        {
            throw new ArgumentException(
                "La fecha inicial no puede ser posterior a la fecha final.",
                nameof(filter));
        }

        filter.Entity = Normalize(filter.Entity);
        filter.Action = Normalize(filter.Action);

        return _auditRepository.GetAllAsync(
            filter,
            cancellationToken);
    }

    public Task<AuditDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(id),
                "El identificador debe ser mayor que cero.");
        }

        return _auditRepository.GetByIdAsync(
            id,
            cancellationToken);
    }

    public async Task<AuditDto> CreateAsync(
        CreateAuditDto audit,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(audit);

        if (audit.UserId.HasValue &&
            audit.UserId.Value <= 0)
        {
            throw new ArgumentException(
                "El identificador del usuario debe ser mayor que cero.",
                nameof(audit));
        }

        if (string.IsNullOrWhiteSpace(audit.Action))
        {
            throw new ArgumentException(
                "La acción es obligatoria.",
                nameof(audit));
        }

        audit.Action = audit.Action.Trim();
        audit.Entity = Normalize(audit.Entity);
        audit.Detail = Normalize(audit.Detail);
        audit.IpAddress = Normalize(audit.IpAddress);

        if (audit.Action.Length > 50)
        {
            throw new ArgumentException(
                "La acción no puede superar 50 caracteres.",
                nameof(audit));
        }

        if (audit.Entity?.Length > 80)
        {
            throw new ArgumentException(
                "La entidad no puede superar 80 caracteres.",
                nameof(audit));
        }

        if (audit.Detail?.Length > 400)
        {
            throw new ArgumentException(
                "El detalle no puede superar 400 caracteres.",
                nameof(audit));
        }

        if (audit.IpAddress?.Length > 45)
        {
            throw new ArgumentException(
                "La dirección IP no puede superar 45 caracteres.",
                nameof(audit));
        }

        if (audit.EntityId.HasValue &&
            audit.EntityId.Value <= 0)
        {
            throw new ArgumentException(
                "El identificador de la entidad debe ser mayor que cero.",
                nameof(audit));
        }

        var auditId = await _auditRepository.CreateAsync(
            audit,
            cancellationToken);

        var createdAudit = await _auditRepository.GetByIdAsync(
            auditId,
            cancellationToken);

        return createdAudit
            ?? throw new InvalidOperationException(
                "El registro de auditoría fue creado, pero no pudo recuperarse.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}