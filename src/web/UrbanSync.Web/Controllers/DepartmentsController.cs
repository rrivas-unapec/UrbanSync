using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Departments;
using UrbanSync.Web.Services;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador,SupervisorOperaciones")]
public sealed class DepartmentsController : Controller
{
    private readonly IDepartmentsPageService _departmentsPageService;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(
        IDepartmentsPageService departmentsPageService,
        ActivityLogger activityLogger,
        ILogger<DepartmentsController> logger)
    {
        _departmentsPageService = departmentsPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = await _departmentsPageService.BuildListAsync(
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        string name,
        int? jurisdictionId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["DepartmentsError"] =
                "El nombre del departamento es obligatorio.";

            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _departmentsPageService.CreateAsync(
                name,
                jurisdictionId,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de departamento",
                $"Se creó el departamento '{name}'.");

            TempData["DepartmentsSuccess"] =
                "El departamento fue creado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación del departamento.");

            TempData["DepartmentsError"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
