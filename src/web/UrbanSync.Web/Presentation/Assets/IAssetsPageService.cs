using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Assets;

public interface IAssetsPageService
{
    Task<AssetsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task<AssetDetailsViewModel?> BuildDetailsAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string code,
        string name,
        string type,
        string status,
        int jurisdictionId,
        DateTime? installationDate,
        CancellationToken cancellationToken = default);
}
