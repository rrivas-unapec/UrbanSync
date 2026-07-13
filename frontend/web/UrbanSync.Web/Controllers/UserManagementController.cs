using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UserManagementController : Controller
{
    private readonly IUrbanSyncApiClient _api;
    private readonly ActivityLogger _activityLogger;

    public UserManagementController(IUrbanSyncApiClient api, ActivityLogger activityLogger)
    {
        _api = api;
        _activityLogger = activityLogger;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _api.GetUsersAsync();

        var result = users.Select(user => new UserListViewModel
        {
            Id = user.Id.ToString(),
            FullName = user.NombreCompleto,
            Email = user.Email,
            Role = user.RolNombre,
            Position = user.RolNombre,
            IsActive = user.Activo
        }).ToList();

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _api.GetRolesAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        ViewBag.Roles = await _api.GetRolesAsync();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var roles = (IReadOnlyList<ApiRoleDto>)ViewBag.Roles;
            var role = roles.FirstOrDefault(r => r.Nombre == model.Role);

            if (role is null)
            {
                ModelState.AddModelError(nameof(model.Role), "Rol no encontrado en la API.");
                return View(model);
            }

            await _api.CreateUserAsync(new ApiCreateUserRequest
            {
                NombreUsuario = model.Email,
                NombreCompleto = model.FullName,
                Email = model.Email,
                Password = model.Password,
                RolId = role.Id
            });

            await _activityLogger.LogAsync("Creacion de usuario", $"Se creo el usuario {model.Email} con rol {model.Role}.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        if (!int.TryParse(id, out var userId))
            return BadRequest();

        await _api.ToggleUserStatusAsync(userId);
        await _activityLogger.LogAsync("Cambio de estado de usuario", $"Se cambio el estado del usuario #{userId}.");

        return RedirectToAction(nameof(Index));
    }
}
