namespace UrbanSync.Web.ApiClients.Claims;

public interface IClaimsApiClient
{
    Task<IReadOnlyList<ClaimResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClaimResponse>> GetByCitizenIdAsync(
        int citizenUserId,
        CancellationToken cancellationToken = default);

    Task<ClaimResponse?> CreateAsync(
        CreateClaimRequest request,
        CancellationToken cancellationToken = default);

    Task<ClaimResponse?> UpdateStatusAsync(
        int id,
        UpdateClaimStatusRequest request,
        CancellationToken cancellationToken = default);
}
