using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Jurisdictions;

public interface IJurisdictionsPageService
{
    Task<JurisdictionsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string name,
        string level,
        int? parentJurisdictionId,
        CancellationToken cancellationToken = default);
}
