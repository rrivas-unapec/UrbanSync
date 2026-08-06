using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Authentication;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.Authentication;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

public sealed class AuthController : Controller
{
    private readonly IAuthenticationApiClient
        _authenticationApiClient;

    private readonly ActivityLogger
        _activityLogger;

    public AuthController(
        IAuthenticationApiClient authenticationApiClient,
        ActivityLogger activityLogger)
    {
        _authenticationApiClient =
            authenticationApiClient;

        _activityLogger =
            activityLogger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        return View();
    }

    [AllowAnonymous]
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
                await _authenticationApiClient
                    .LoginAsync(
                        new LoginRequest
                        {
                            Email =
                                model.Email.Trim(),
                            Password =
                                model.Password
                        },
                        cancellationToken);

            if (response is null ||
                string.IsNullOrWhiteSpace(
                    response.Token))
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
                    WebClaimTypes.AccessToken,
                    response.Token)
            };

            var identity =
                new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults
                        .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent =
                        model.RememberMe,

                    AllowRefresh = false,

                    ExpiresUtc =
                        response.ExpiresAtUtc
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

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(
                "Index",
                "Dashboard");
        }

        return View();
    }

    [AllowAnonymous]
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
            await _authenticationApiClient
                .RegisterAsync(
                    new RegisterRequest
                    {
                        NombreCompleto =
                            model.FullName.Trim(),

                        Email =
                            model.Email.Trim(),

                        Password =
                            model.Password
                    },
                    cancellationToken);

            await _activityLogger.LogAsync(
                "Registro",
                "Un usuario se registró como ciudadano.");

            TempData["RegistrationSuccess"] =
                "Tu cuenta fue creada correctamente. Ya puedes iniciar sesión.";

            return RedirectToAction(
                nameof(Login));
        }
        catch (UrbanSyncApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(
            new ChangePasswordViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (string.Equals(
                model.CurrentPassword,
                model.NewPassword,
                StringComparison.Ordinal))
        {
            ModelState.AddModelError(
                nameof(model.NewPassword),
                "La nueva contraseña debe ser diferente de la contraseña actual.");

            return View(model);
        }

        try
        {
            await _authenticationApiClient
                .ChangePasswordAsync(
                    new ChangePasswordRequest
                    {
                        CurrentPassword =
                            model.CurrentPassword,

                        NewPassword =
                            model.NewPassword,

                        ConfirmNewPassword =
                            model.ConfirmNewPassword
                    },
                    cancellationToken);

            await _activityLogger.LogAsync(
                "Cambio de contraseña",
                "El usuario cambió su contraseña.");

            TempData["PasswordChanged"] =
                "Tu contraseña fue actualizada correctamente.";

            return RedirectToAction(
                nameof(ChangePassword));
        }
        catch (UrbanSyncApiException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            return View(model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _activityLogger.LogAsync(
            "Cierre de sesión",
            "El usuario cerró sesión.");

        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults
                .AuthenticationScheme);

        return RedirectToAction(
            nameof(Login),
            "Auth");
    }
}