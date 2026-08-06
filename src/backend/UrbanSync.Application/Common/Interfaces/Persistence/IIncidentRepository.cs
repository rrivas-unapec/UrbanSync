using UrbanSync.Application.Features.Incidents;

namespace UrbanSync.Application.Common.Interfaces.Persistence;

public interface IIncidentRepository
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(
        string? status = null,
        int? reportingUserId = null,
        CancellationToken cancellationToken = default);

    Task<IncidentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateIncidentDto incident,
        int reportingUserId,
        string caseCode,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        int id,
        string status,
        int? assignedInstitutionId,
        CancellationToken cancellationToken = default);

    Task<bool> TriageAsync(
        int id,
        TriageIncidentDto incident,
        string? resultingStatus,
        CancellationToken cancellationToken = default);
}