using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.ApiClients.WorkOrders;
using UrbanSync.Web.Presentation.Dashboard;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private const string JobManagementRoles =
        "Administrador,SupervisorOperaciones";

    private static readonly Dictionary<string, string>
        ModerationActions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["aprobar"] = "asignar",
                ["rechazar"] = "rechazar"
            };

    private readonly IDashboardPageService _dashboardPageService;
    private readonly IIncidentsApiClient _incidentsApiClient;
    private readonly IWorkOrdersApiClient _workOrdersApiClient;
    private readonly IUsersApiClient _usersApiClient;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardPageService dashboardPageService,
        IIncidentsApiClient incidentsApiClient,
        IWorkOrdersApiClient workOrdersApiClient,
        IUsersApiClient usersApiClient,
        ActivityLogger activityLogger,
        ILogger<DashboardController> logger)
    {
        _dashboardPageService = dashboardPageService;
        _incidentsApiClient = incidentsApiClient;
        _workOrdersApiClient = workOrdersApiClient;
        _usersApiClient = usersApiClient;
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
            var supervisorModel =
                await _dashboardPageService.BuildMainPanelAsync(
                    cancellationToken);

            return View("Supervisor", supervisorModel);
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

    [Authorize(Roles = "Administrador,SupervisorOperaciones")]
    public async Task<IActionResult> Mapa(
        CancellationToken cancellationToken)
    {
        var model = await _dashboardPageService.BuildMapAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador,SupervisorOperaciones")]
    public async Task<IActionResult> ActivosOrdenes(
        CancellationToken cancellationToken)
    {
        var model =
            await _dashboardPageService.BuildActiveWorkOrdersAsync(
                cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador,SupervisorOperaciones")]
    public async Task<IActionResult> Rutas(
        CancellationToken cancellationToken)
    {
        var model = await _dashboardPageService.BuildRoutesAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = JobManagementRoles)]
    [HttpGet]
    public async Task<IActionResult> CreateJob(
        CancellationToken cancellationToken)
    {
        var model = await BuildCreateJobPageAsync(cancellationToken);

        return View(model);
    }

    [Authorize(Roles = JobManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(
        int incidentId,
        int assignedUserId,
        string jobDescription,
        DateTime? startDate,
        CancellationToken cancellationToken)
    {
        if (incidentId <= 0 ||
            assignedUserId <= 0 ||
            string.IsNullOrWhiteSpace(jobDescription))
        {
            ModelState.AddModelError(
                string.Empty,
                "Incidencia, técnico asignado y descripción son obligatorios.");

            var model = await BuildCreateJobPageAsync(cancellationToken);

            return View(model);
        }

        try
        {
            await _workOrdersApiClient.CreateAsync(
                new CreateWorkOrderRequest
                {
                    IncidentId = incidentId,
                    AssignedUserId = assignedUserId,
                    JobDescription = jobDescription,
                    StartDate = startDate
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de trabajo",
                $"Se creó un trabajo para la incidencia #{incidentId}.");

            TempData["ActivosOrdenesSuccess"] =
                "El trabajo fue creado correctamente.";

            return RedirectToAction(nameof(ActivosOrdenes));
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación del trabajo.");

            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            var model = await BuildCreateJobPageAsync(cancellationToken);

            return View(model);
        }
    }

    [Authorize(Roles = JobManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateJobStatus(
        int id,
        string estado,
        string? resultado,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            TempData["ActivosOrdenesError"] =
                "Debes seleccionar un estado.";

            return RedirectToAction(nameof(ActivosOrdenes));
        }

        try
        {
            await _workOrdersApiClient.UpdateStatusAsync(
                id,
                new UpdateWorkOrderStatusRequest
                {
                    Status = estado,
                    Result = resultado
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Actualización de trabajo",
                $"Se cambió el estado del trabajo #{id} a '{estado}'.");

            TempData["ActivosOrdenesSuccess"] =
                "El trabajo fue actualizado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la actualización del trabajo {Id}.",
                id);

            TempData["ActivosOrdenesError"] = exception.Message;
        }

        return RedirectToAction(nameof(ActivosOrdenes));
    }

    [Authorize(Roles = "Administrador,AnalistaTecnico,SupervisorOperaciones")]
    public async Task<IActionResult> Moderacion(
        CancellationToken cancellationToken)
    {
        var model =
            await _dashboardPageService.BuildModerationQueueAsync(
                cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador,AnalistaTecnico,SupervisorOperaciones")]
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

    private async Task<CreateJobPageViewModel> BuildCreateJobPageAsync(
        CancellationToken cancellationToken)
    {
        var incidentsTask = _incidentsApiClient.GetAllAsync(
            cancellationToken: cancellationToken);

        var usersTask = _usersApiClient.GetAllAsync(
            cancellationToken);

        await Task.WhenAll(incidentsTask, usersTask);

        return new CreateJobPageViewModel
        {
            Incidencias = incidentsTask.Result
                .OrderByDescending(incident => incident.FechaReporte)
                .Select(incident => new IncidentOptionViewModel
                {
                    Id = incident.Id,
                    CodigoCaso = incident.CodigoCaso,
                    TipoIncidencia = incident.TipoIncidencia
                })
                .ToList(),
            Usuarios = usersTask.Result
                .Where(user => user.Activo)
                .OrderBy(user => user.NombreCompleto)
                .Select(user => new UserOptionViewModel
                {
                    Id = user.Id,
                    NombreCompleto = user.NombreCompleto
                })
                .ToList()
        };
    }
}