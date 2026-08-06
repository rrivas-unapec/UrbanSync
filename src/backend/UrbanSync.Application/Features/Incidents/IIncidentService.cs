namespace UrbanSync.Application.Features.Incidents;

public interface IIncidentService
{
    Task<IReadOnlyList<IncidentDto>> GetAllAsync(
        string? status = null,
        int? reportingUserId = null,
        CancellationToken cancellationToken = default);

    Task<IncidentDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IncidentDto> CreateAsync(
        CreateIncidentDto incident,
        int reportingUserId,
        CancellationToken cancellationToken = default);

    Task<IncidentDto?> UpdateStatusAsync(
        int id,
        UpdateIncidentStatusDto incident,
        int actingUserId,
        CancellationToken cancellationToken = default);

    Task<IncidentDto?> TriageAsync(
        int id,
        TriageIncidentDto incident,
        int actingUserId,
        CancellationToken cancellationToken = default);
}