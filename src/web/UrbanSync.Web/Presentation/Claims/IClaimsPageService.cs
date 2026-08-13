using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Claims;

public interface IClaimsPageService
{
    Task<ClaimsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        int id,
        string status,
        CancellationToken cancellationToken = default);
}
