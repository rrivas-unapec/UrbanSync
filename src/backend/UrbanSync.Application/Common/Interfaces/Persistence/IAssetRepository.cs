using UrbanSync.Application.Features.Asset;

namespace UrbanSync.Application.Common.Interfaces.Persistence;

public interface IAssetRepository
{
    Task<IReadOnlyList<AssetDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AssetDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssetHistoryDto>>
        GetHistoryByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        CreateAssetDto asset,
        CancellationToken cancellationToken = default);
}