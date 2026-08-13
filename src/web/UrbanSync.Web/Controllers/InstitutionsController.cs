using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Institutions;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones")]
public sealed class InstitutionsController : Controller
{
    private readonly IInstitutionsPageService _institutionsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<InstitutionsController> _logger;

    public InstitutionsController(
        IInstitutionsPageService institutionsPageService,
        ActivityLogger activityLogger,
        ILogger<InstitutionsController> logger)
    {
        _institutionsPageService = institutionsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _institutionsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        string institutionType,
        string? contactEmail,
        string? contactPhone,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(institutionType))
        {
            TempData["InstitutionsError"] =
                "El nombre y el tipo de institución son obligatorios.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _institutionsPageService.CreateAsync(
                name,
                institutionType,
                contactEmail,
                contactPhone,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de institución",
                $"Se creó la institución '{name}'.");

            TempData["InstitutionsSuccess"] =
                "La institución fue creada correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación de la institución.");

            TempData["InstitutionsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
