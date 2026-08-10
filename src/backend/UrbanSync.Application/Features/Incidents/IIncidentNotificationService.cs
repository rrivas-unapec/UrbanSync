namespace UrbanSync.Application.Features.Incidents;

public interface IIncidentNotificationService
{
    Task NotifyStatusChangedAsync(
        IncidentDto previousIncident,
        IncidentDto currentIncident,
        CancellationToken cancellationToken = default);
}