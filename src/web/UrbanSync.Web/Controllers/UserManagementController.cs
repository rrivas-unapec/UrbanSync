using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Presentation.Users;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class UserManagementController : Controller
{
    private readonly IUserManagementPageService
        _userManagementPageService;

    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserManagementPageService userManagementPageService,
        ActivityLogger activityLogger,
        ILogger<UserManagementController> logger)
    {
        _userManagementPageService = userManagementPageService;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model =
            await _userManagementPageService.BuildListAsync(
                cancellationToken);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model =
            await _userManagementPageService.BuildCreatePageAsync(
                cancellationToken: cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UserCreatePageViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model = await _userManagementPageService
                .BuildCreatePageAsync(
                    model.Form,
                    cancellationToken);

            return View(model);
        }

        try
        {
            await _userManagementPageService.CreateAsync(
                model.Form,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de usuario",
                $"Se creó el usuario {model.Form.Email}.");

            return RedirectToAction(nameof(Index));
        }
        catch (UrbanSyncApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            model = await _userManagementPageService
                .BuildCreatePageAsync(
                    model.Form,
                    cancellationToken);

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        try
        {
            await _userManagementPageService.ToggleStatusAsync(
                id,
                cancellationToken);

            await _activityLogger.LogAsync(
                "Cambio de estado de usuario",
                $"Se cambió el estado del usuario #{id}.");
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó el cambio de estado del usuario {Id}.",
                id);

            TempData["UserManagementError"] =
                exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}