using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Assets;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones,AnalistaTecnico,GestorUbicacion")]
public sealed class AssetsController : Controller
{
    private readonly IAssetsPageService _assetsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(
        IAssetsPageService assetsPageService,
        ActivityLogger activityLogger,
        ILogger<AssetsController> logger)
    {
        _assetsPageService = assetsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _assetsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        var model = await _assetsPageService.BuildDetailsAsync(
            id,
            cancellationToken);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize(Roles = "Administrador,SupervisorOperaciones")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string code,
        string name,
        string type,
        string status,
        int jurisdictionId,
        DateTime? installationDate,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(type) ||
            jurisdictionId <= 0)
        {
            TempData["AssetsError"] =
                "Código, nombre, tipo y jurisdicción son obligatorios.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _assetsPageService.CreateAsync(
                code,
                name,
                type,
                string.IsNullOrWhiteSpace(status) ? "Operativo" : status,
                jurisdictionId,
                installationDate,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de activo urbano",
                $"Se creó el activo '{code} - {name}'.");

            TempData["AssetsSuccess"] =
                "El activo urbano fue creado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación del activo urbano.");

            TempData["AssetsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
