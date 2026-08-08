using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Incidents;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class IncidentsController : Controller
{
    private const string ManagementRoles =
        "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly IIncidentsApiClient
        _incidentsApiClient;

    private readonly ILogger<IncidentsController>
        _logger;

    public IncidentsController(
        IIncidentsApiClient incidentsApiClient,
        ILogger<IncidentsController> logger)
    {
        _incidentsApiClient = incidentsApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        CancellationToken cancellationToken)
    {
        var canManage =
            User.IsInRole("Administrador") ||
            User.IsInRole("SupervisorOperaciones") ||
            User.IsInRole("AnalistaTecnico");

        ViewBag.Status = status;
        ViewBag.MineOnly = !canManage;

        try
        {
            var incidents =
                await _incidentsApiClient.GetAllAsync(
                    status,
                    mine: !canManage,
                    cancellationToken);

            return View(incidents);
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron consultar las incidencias.");

            ViewBag.Error = exception.Message;

            return View(
                Array.Empty<IncidentResponse>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var incident =
                await _incidentsApiClient.GetByIdAsync(
                    id,
                    cancellationToken);

            if (incident is null)
            {
                return NotFound();
            }

            return View(incident);
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar la incidencia {IncidentId}.",
                id);

            TempData["IncidentError"] =
                exception.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        string estado,
        int? institucionAsignadaId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            TempData["IncidentError"] =
                "Debes seleccionar un estado.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        try
        {
            await _incidentsApiClient.UpdateStatusAsync(
                id,
                new UpdateIncidentStatusRequest
                {
                    Estado = estado,
                    InstitucionAsignadaId =
                        institucionAsignadaId
                },
                cancellationToken);

            TempData["IncidentSuccess"] =
                "El estado de la incidencia fue actualizado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo actualizar la incidencia {IncidentId}.",
                id);

            TempData["IncidentError"] =
                exception.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}