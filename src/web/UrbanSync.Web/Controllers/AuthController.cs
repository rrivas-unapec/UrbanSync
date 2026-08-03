using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Authentication;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Roles;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

public sealed class AuthController : Controller
{
    private readonly IAuthenticationApiClient _authenticationApiClient;
    private readonly IUsersApiClient _usersApiClient;
    private readonly IRolesApiClient _rolesApiClient;
    private readonly ActivityLogger _activityLogger;

    public AuthController(
        IAuthenticationApiClient authenticationApiClient,
        IUsersApiClient usersApiClient,
        IRolesApiClient rolesApiClient,
        ActivityLogger activityLogger)
    {
        _authenticationApiClient = authenticationApiClient;
        _usersApiClient = usersApiClient;
        _rolesApiClient = rolesApiClient;
        _activityLogger = activityLogger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var response =
                await _authenticationApiClient.LoginAsync(
                    new LoginRequest
                    {
                        Email = model.Email.Trim(),
                        Password = model.Password
                    },
                    cancellationToken);

            if (response is null ||
                string.IsNullOrWhiteSpace(response.Token))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Usuario o contraseña incorrectos.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    response.User.Id.ToString()),

                new(
                    ClaimTypes.Name,
                    response.User.NombreCompleto),

                new(
                    ClaimTypes.Email,
                    response.User.Email),

                new(
                    ClaimTypes.Role,
                    response.User.RolNombre),

                new(
                    "access_token",
                    response.Token)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc =
                        DateTimeOffset.UtcNow.AddHours(8)
                });

            await _activityLogger.LogAsync(
                "Inicio de sesión",
                "El usuario inició sesión desde la web.");

            return RedirectToAction(
                "Index",
                "Dashboard");
        }
        catch (UrbanSyncApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

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
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var roles = await _rolesApiClient.GetAllAsync(
                cancellationToken);

            var ciudadanoRole = roles.FirstOrDefault(
                role => role.Nombre == "Ciudadano");

            if (ciudadanoRole is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "La API no tiene configurado el rol Ciudadano.");

                return View(model);
            }

            await _usersApiClient.CreateAsync(
                new CreateUserRequest
                {
                    NombreUsuario = model.Email.Trim(),
                    NombreCompleto = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    RolId = ciudadanoRole.Id
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Registro",
                "El usuario se registró como ciudadano.");

            return RedirectToAction(nameof(Login));
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
    public async Task<IActionResult> Logout()
    {
        await _activityLogger.LogAsync(
            "Cierre de sesión",
            "El usuario cerró sesión.");

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction(
            nameof(Login),
            "Auth");
    }
}