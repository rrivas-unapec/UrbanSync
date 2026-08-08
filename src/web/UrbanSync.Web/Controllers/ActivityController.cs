using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Activity;
using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.Controllers;

[Authorize(
    Roles =
        "Administrador,SupervisorOperaciones")]
public sealed class ActivityController : Controller
{
    private readonly IActivityApiClient
        _activityApiClient;

    private readonly ILogger<ActivityController>
        _logger;

    public ActivityController(
        IActivityApiClient activityApiClient,
        ILogger<ActivityController> logger)
    {
        _activityApiClient = activityApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int? usuarioId,
        string? entidad,
        string? accion,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        CancellationToken cancellationToken)
    {
        ViewBag.UsuarioId = usuarioId;
        ViewBag.Entidad = entidad;
        ViewBag.Accion = accion;
        ViewBag.FechaInicio = fechaInicio;
        ViewBag.FechaFin = fechaFin;

        try
        {
            var activities =
                await _activityApiClient.GetAllAsync(
                    usuarioId,
                    entidad,
                    accion,
                    fechaInicio,
                    fechaFin,
                    cancellationToken);

            return View(activities);
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron consultar las actividades de auditoría.");

            ViewBag.Error = exception.Message;

            return View(
                Array.Empty<ActivityResponse>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        try
        {
            var activity =
                await _activityApiClient.GetByIdAsync(
                    id,
                    cancellationToken);

            if (activity is null)
            {
                return NotFound();
            }

            return View(activity);
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar la actividad {ActivityId}.",
                id);

            TempData["ActivityError"] =
                exception.Message;

            return RedirectToAction(
                nameof(Index));
        }
    }
}