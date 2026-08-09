
namespace UrbanSync.Application.Features.Asset
{
    public interface IAssetService
    {
        Task<IReadOnlyList<AssetDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<AssetDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AssetHistoryDto?> GetHistoryByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<AssetDto> CreateAsync(
            CreateAssetDto dto,
            CancellationToken cancellationToken = default);
    }
}
