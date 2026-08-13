using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Locations;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones")]
public sealed class LocationsController : Controller
{
    private readonly ILocationsPageService _locationsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(
        ILocationsPageService locationsPageService,
        ActivityLogger activityLogger,
        ILogger<LocationsController> logger)
    {
        _locationsPageService = locationsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _locationsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string address,
        string? reference,
        decimal? latitude,
        decimal? longitude,
        int jurisdictionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(address) ||
            jurisdictionId <= 0)
        {
            TempData["LocationsError"] =
                "La dirección y la jurisdicción son obligatorias.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _locationsPageService.CreateAsync(
                address,
                reference,
                latitude,
                longitude,
                jurisdictionId,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de ubicación",
                $"Se creó la ubicación '{address}'.");

            TempData["LocationsSuccess"] =
                "La ubicación fue creada correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación de la ubicación.");

            TempData["LocationsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
