using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Claims;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Locations;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class MyClaimsController : Controller
{
    private readonly IClaimsApiClient _claimsApiClient;
    private readonly ILocationsApiClient _locationsApiClient;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<MyClaimsController> _logger;

    public MyClaimsController(
        IClaimsApiClient claimsApiClient,
        ILocationsApiClient locationsApiClient,
        ActivityLogger activityLogger,
        ILogger<MyClaimsController> logger)
    {
        _claimsApiClient = claimsApiClient;
        _locationsApiClient = locationsApiClient;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var model = new MyClaimsViewModel();

        try
        {
            var claims = await _claimsApiClient.GetByCitizenIdAsync(
                GetAuthenticatedUserId(),
                cancellationToken);

            model.DatosDisponibles = true;
            model.Reclamaciones = claims
                .Select(claim => new ClaimItemViewModel
                {
                    Id = claim.Id,
                    CitizenUserName = claim.CitizenUserName,
                    LocationAddress = claim.LocationAddress,
                    Category = claim.Category,
                    Title = claim.Title,
                    Description = claim.Description,
                    Status = claim.Status,
                    CreatedAt = claim.CreatedAt
                })
                .ToList();
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron consultar las reclamaciones del ciudadano.");

            model.DatosDisponibles = false;
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken)
    {
        var model = await BuildCreatePageAsync(cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int locationId,
        string category,
        string title,
        string description,
        CancellationToken cancellationToken)
    {
        if (locationId <= 0 ||
            string.IsNullOrWhiteSpace(category) ||
            string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(description))
        {
            ModelState.AddModelError(
                string.Empty,
                "Todos los campos son obligatorios.");

            var model = await BuildCreatePageAsync(cancellationToken);

            return View(model);
        }

        try
        {
            await _claimsApiClient.CreateAsync(
                new CreateClaimRequest
                {
                    CitizenUserId = GetAuthenticatedUserId(),
                    LocationId = locationId,
                    Category = category,
                    Title = title,
                    Description = description
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Creación de reclamación",
                $"Se creó la reclamación '{title}'.");

            TempData["MyClaimsSuccess"] =
                "Tu reclamación fue registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la creación de la reclamación.");

            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            var model = await BuildCreatePageAsync(cancellationToken);

            return View(model);
        }
    }

    private async Task<CreateClaimPageViewModel> BuildCreatePageAsync(
        CancellationToken cancellationToken)
    {
        var locations = await _locationsApiClient.GetAllAsync(
            cancellationToken);

        return new CreateClaimPageViewModel
        {
            Ubicaciones = locations
                .OrderBy(location => location.Address)
                .Select(location => new LocationOptionViewModel
                {
                    Id = location.Id,
                    Address = location.Address,
                    JurisdictionName = location.JurisdictionName
                })
                .ToList()
        };
    }

    private int GetAuthenticatedUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(value!);
    }
}
