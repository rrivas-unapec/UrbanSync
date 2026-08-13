namespace UrbanSync.Web.ApiClients.Jurisdictions;

public interface IJurisdictionsApiClient
{
    Task<IReadOnlyList<JurisdictionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<JurisdictionResponse?> CreateAsync(
        CreateJurisdictionRequest request,
        CancellationToken cancellationToken = default);
}
