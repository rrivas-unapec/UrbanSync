using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Dashboard;

public interface IDashboardPageService
{
    Task<PanelPrincipalViewModel> BuildMainPanelAsync(
        CancellationToken cancellationToken = default);

    Task<MapaViewModel> BuildMapAsync(
        CancellationToken cancellationToken = default);

    Task<ActivosOrdenesViewModel> BuildActiveWorkOrdersAsync(
        CancellationToken cancellationToken = default);

    Task<RutasViewModel> BuildRoutesAsync(
        CancellationToken cancellationToken = default);

    Task<ModeracionViewModel> BuildModerationQueueAsync(
        CancellationToken cancellationToken = default);
}