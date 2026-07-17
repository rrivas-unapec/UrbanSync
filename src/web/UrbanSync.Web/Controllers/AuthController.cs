using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

public class AuthController : Controller
{
    private readonly IUrbanSyncApiClient _api;
    private readonly ActivityLogger _activityLogger;

    public AuthController(IUrbanSyncApiClient api, ActivityLogger activityLogger)
    {
        _api = api;
        _activityLogger = activityLogger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _api.LoginAsync(new ApiLoginRequest
            {
                Email = model.Email,
                Password = model.Password
            });

            if (response is null || string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new(ClaimTypes.Name, response.User.NombreCompleto),
                new(ClaimTypes.Email, response.User.Email),
                new(ClaimTypes.Role, response.User.RolNombre),
                new("access_token", response.Token)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            await _activityLogger.LogAsync("Inicio de sesion", "El usuario inicio sesion desde el monolito conectado a la API.");

            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var roles = await _api.GetRolesAsync();
            var ciudadanoRole = roles.FirstOrDefault(r => r.Nombre == "Ciudadano");

            if (ciudadanoRole is null)
            {
                ModelState.AddModelError(string.Empty, "La API no tiene configurado el rol Ciudadano.");
                return View(model);
            }

            await _api.CreateUserAsync(new ApiCreateUserRequest
            {
                NombreUsuario = model.Email,
                NombreCompleto = model.FullName,
                Email = model.Email,
                Password = model.Password,
                RolId = ciudadanoRole.Id
            });

            await _activityLogger.LogAsync("Registro", "El usuario se registro como ciudadano desde el monolito conectado a la API.");

            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _activityLogger.LogAsync("Cierre de sesion", "El usuario cerro sesion.");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }
}
