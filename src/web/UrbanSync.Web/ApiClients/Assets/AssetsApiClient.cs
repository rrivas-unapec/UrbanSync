using System.Net;
using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Assets;

public sealed class AssetsApiClient
    : ApiClientBase,
      IAssetsApiClient
{
    public AssetsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<AssetResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var assets = await GetAsync<List<AssetResponse>>(
            "api/assets",
            cancellationToken);

        return assets ?? [];
    }

    public async Task<AssetResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<AssetResponse>(
                $"api/assets/{id}",
                cancellationToken);
        }
        catch (UrbanSyncApiException exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<AssetHistoryResponse>?> GetHistoryAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAsync<List<AssetHistoryResponse>>(
                $"api/assets/{id}/history",
                cancellationToken);
        }
        catch (UrbanSyncApiException exception)
            when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task<AssetResponse?> CreateAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateAssetRequest, AssetResponse>(
            "api/assets",
            request,
            cancellationToken);
    }
}
