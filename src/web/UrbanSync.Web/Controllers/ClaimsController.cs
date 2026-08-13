using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Claims;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones")]
public sealed class ClaimsController : Controller
{
    private static readonly HashSet<string> ValidStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Pendiente",
            "EnRevision",
            "Resuelta",
            "Rechazada"
        };

    private readonly IClaimsPageService _claimsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<ClaimsController> _logger;

    public ClaimsController(
        IClaimsPageService claimsPageService,
        ActivityLogger activityLogger,
        ILogger<ClaimsController> logger)
    {
        _claimsPageService = claimsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _claimsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        string status,
        CancellationToken cancellationToken)
    {
        if (id <= 0 || !ValidStatuses.Contains(status))
        {
            return BadRequest();
        }

        try
        {
            await _claimsPageService.UpdateStatusAsync(
                id,
                status,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Actualización de reclamación",
                $"Se cambió el estado de la reclamación #{id} a '{status}'.");
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la actualización de la reclamación {Id}.",
                id);

            TempData["ClaimsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
