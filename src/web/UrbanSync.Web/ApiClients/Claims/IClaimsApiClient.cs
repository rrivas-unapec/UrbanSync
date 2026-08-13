namespace UrbanSync.Web.ApiClients.Claims;

public interface IClaimsApiClient
{
    Task<IReadOnlyList<ClaimResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ClaimResponse?> UpdateStatusAsync(
        int id,
        UpdateClaimStatusRequest request,
        CancellationToken cancellationToken = default);
}
