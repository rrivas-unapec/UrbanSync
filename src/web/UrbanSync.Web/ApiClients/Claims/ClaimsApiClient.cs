using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Claims;

public sealed class ClaimsApiClient
    : ApiClientBase,
      IClaimsApiClient
{
    public ClaimsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<ClaimResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var claims = await GetAsync<List<ClaimResponse>>(
            "api/claims",
            cancellationToken);

        return claims ?? [];
    }

    public Task<ClaimResponse?> UpdateStatusAsync(
        int id,
        UpdateClaimStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<UpdateClaimStatusRequest, ClaimResponse>(
            $"api/claims/{id}/status",
            request,
            cancellationToken);
    }
}
