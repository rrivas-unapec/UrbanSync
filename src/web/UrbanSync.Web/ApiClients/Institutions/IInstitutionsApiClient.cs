namespace UrbanSync.Web.ApiClients.Institutions;

public interface IInstitutionsApiClient
{
    Task<IReadOnlyList<InstitutionResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<InstitutionResponse?> CreateAsync(
        CreateInstitutionRequest request,
        CancellationToken cancellationToken = default);
}
