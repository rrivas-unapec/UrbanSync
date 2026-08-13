namespace UrbanSync.Web.ApiClients.Assets;

public interface IAssetsApiClient
{
    Task<IReadOnlyList<AssetResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AssetResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetHistoryResponse>?> GetHistoryAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<AssetResponse?> CreateAsync(
        CreateAssetRequest request,
        CancellationToken cancellationToken = default);
}
