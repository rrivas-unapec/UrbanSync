using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.Presentation.Dashboard;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private static readonly Dictionary<string, string>
        ModerationActions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["aprobar"] = "asignar",
                ["rechazar"] = "rechazar"
            };

    private readonly IDashboardPageService _dashboardPageService;
    private readonly IIncidentsApiClient _incidentsApiClient;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardPageService dashboardPageService,
        IIncidentsApiClient incidentsApiClient,
        ActivityLogger activityLogger,
        ILogger<DashboardController> logger)
    {
        _dashboardPageService = dashboardPageService;
        _incidentsApiClient = incidentsApiClient;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            var model =
                await _dashboardPageService.BuildMainPanelAsync(
                    cancellationToken);

            return View("Administrador", model);
        }

        if (User.IsInRole("SupervisorOperaciones"))
        {
            return View("Supervisor");
        }

        if (User.IsInRole("AnalistaTecnico"))
        {
            var indicators =
                await _dashboardPageService.BuildTechnicalIndicatorsAsync(
                    cancellationToken);

            return View("Tecnico", indicators);
        }

        if (User.IsInRole("Ciudadano"))
        {
            return View("Ciudadano");
        }

        return View("SinPanel");
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Mapa(
        CancellationToken cancellationToken)
    {
        var model = await _dashboardPageService.BuildMapAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivosOrdenes(
        CancellationToken cancellationToken)
    {
        var model =
            await _dashboardPageService.BuildActiveWorkOrdersAsync(
                cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Rutas(
        CancellationToken cancellationToken)
    {
        var model = await _dashboardPageService.BuildRoutesAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador,AnalistaTecnico")]
    public async Task<IActionResult> Moderacion(
        CancellationToken cancellationToken)
    {
        var model =
            await _dashboardPageService.BuildModerationQueueAsync(
                cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador,AnalistaTecnico")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModeracionAccion(
        int id,
        string accion,
        int? tipoIncidenciaId,
        string? prioridad,
        CancellationToken cancellationToken)
    {
        if (!ModerationActions.TryGetValue(
                accion,
                out var apiAction))
        {
            return BadRequest();
        }

        try
        {
            await _incidentsApiClient.TriageAsync(
                id,
                new TriageIncidentRequest
                {
                    Accion = apiAction,
                    TipoIncidenciaId = tipoIncidenciaId,
                    Prioridad = string.IsNullOrWhiteSpace(prioridad)
                        ? null
                        : prioridad
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Moderación de reporte",
                $"Se aplicó la acción '{accion}' al reporte #{id}.");
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la moderación del reporte {Id}.",
                id);

            TempData["ModeracionError"] = exception.Message;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo moderar el reporte {Id}.",
                id);

            TempData["ModeracionError"] =
                "No se pudo comunicar con la API de UrbanSync.";
        }

        return RedirectToAction(nameof(Moderacion));
    }
}