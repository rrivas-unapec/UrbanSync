using UrbanSync.Application.Features.Audit;

namespace UrbanSync.Application.Common.Interfaces.Persistence;

public interface IAuditRepository
{
    Task<IReadOnlyList<AuditDto>> GetAllAsync(
        AuditFilterDto? filter = null,
        CancellationToken cancellationToken = default);

    Task<AuditDto?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<long> CreateAsync(
        CreateAuditDto audit,
        CancellationToken cancellationToken = default);
}