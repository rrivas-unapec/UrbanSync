using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Jurisdictions;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones")]
public sealed class JurisdictionsController : Controller
{
    private readonly IJurisdictionsPageService _jurisdictionsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<JurisdictionsController> _logger;

    public JurisdictionsController(
        IJurisdictionsPageService jurisdictionsPageService,
        ActivityLogger activityLogger,
        ILogger<JurisdictionsController> logger)
    {
        _jurisdictionsPageService = jurisdictionsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _jurisdictionsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        string level,
        int? parentJurisdictionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(level))
        {
            TempData["JurisdictionsError"] =
                "El nombre y el nivel son obligatorios.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _jurisdictionsPageService.CreateAsync(
                name,
                level,
                parentJurisdictionId,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de jurisdicción",
                $"Se creó la jurisdicción '{name}'.");

            TempData["JurisdictionsSuccess"] =
                "La jurisdicción fue creada correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación de la jurisdicción.");

            TempData["JurisdictionsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
