using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Locations;

public interface ILocationsPageService
{
    Task<LocationsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        string address,
        string? reference,
        decimal? latitude,
        decimal? longitude,
        int jurisdictionId,
        CancellationToken cancellationToken = default);
}
