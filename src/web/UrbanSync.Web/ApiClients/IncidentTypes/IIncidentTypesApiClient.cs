namespace UrbanSync.Web.ApiClients.IncidentTypes;

public interface IIncidentTypesApiClient
{
    Task<IReadOnlyList<IncidentTypeResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
