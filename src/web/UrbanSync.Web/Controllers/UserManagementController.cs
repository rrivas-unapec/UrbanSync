using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Roles;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador")]
public sealed class UserManagementController : Controller
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IRolesApiClient _rolesApiClient;
    private readonly ActivityLogger _activityLogger;

    public UserManagementController(
        IUsersApiClient usersApiClient,
        IRolesApiClient rolesApiClient,
        ActivityLogger activityLogger)
    {
        _usersApiClient = usersApiClient;
        _rolesApiClient = rolesApiClient;
        _activityLogger = activityLogger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var users = await _usersApiClient.GetAllAsync(
            cancellationToken);

        var model = users.Select(user =>
            new UserListViewModel
            {
                Id = user.Id.ToString(),
                FullName = user.NombreCompleto,
                Email = user.Email,
                Role = user.RolNombre,
                Position = user.RolNombre,
                IsActive = user.Activo
            })
            .ToList();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        await LoadRolesAsync(cancellationToken);

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UserCreateViewModel model,
        CancellationToken cancellationToken)
    {
        var roles = await LoadRolesAsync(
            cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var role = roles.FirstOrDefault(
            item => item.Nombre == model.Role);

        if (role is null)
        {
            ModelState.AddModelError(
                nameof(model.Role),
                "Rol no encontrado en la API.");

            return View(model);
        }

        try
        {
            await _usersApiClient.CreateAsync(
                new CreateUserRequest
                {
                    NombreUsuario = model.Email.Trim(),
                    NombreCompleto = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    RolId = role.Id
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de usuario",
                $"Se creó el usuario {model.Email} con rol {model.Role}.");

            return RedirectToAction(nameof(Index));
        }
        catch (UrbanSyncApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(
        string id,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(id, out var userId))
        {
            return BadRequest();
        }

        await _usersApiClient.ToggleStatusAsync(
            userId,
            cancellationToken);

        await _activityLogger.LogAsync(
            "Cambio de estado de usuario",
            $"Se cambió el estado del usuario #{userId}.");

        return RedirectToAction(nameof(Index));
    }

    private async Task<IReadOnlyList<RoleResponse>> LoadRolesAsync(
        CancellationToken cancellationToken)
    {
        var roles = await _rolesApiClient.GetAllAsync(
            cancellationToken);

        ViewBag.Roles = roles;

        return roles;
    }
}