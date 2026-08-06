namespace UrbanSync.Application.Features.Audit;

public interface IAuditService
{
    Task<IReadOnlyList<AuditDto>> GetAllAsync(
        AuditFilterDto? filter = null,
        CancellationToken cancellationToken = default);

    Task<AuditDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<AuditDto> CreateAsync(
        CreateAuditDto audit,
        CancellationToken cancellationToken = default);
}