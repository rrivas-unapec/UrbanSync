using UrbanSync.Web.ApiClients.Jurisdictions;
using UrbanSync.Web.ApiClients.Locations;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Locations;

public sealed class LocationsPageService : ILocationsPageService
{
    private readonly ILocationsApiClient _locationsApiClient;
    private readonly IJurisdictionsApiClient _jurisdictionsApiClient;
    private readonly ILogger<LocationsPageService> _logger;

    public LocationsPageService(
        ILocationsApiClient locationsApiClient,
        IJurisdictionsApiClient jurisdictionsApiClient,
        ILogger<LocationsPageService> logger)
    {
        _locationsApiClient = locationsApiClient;
        _jurisdictionsApiClient = jurisdictionsApiClient;
        _logger = logger;
    }

    public async Task<LocationsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var locationsTask = _locationsApiClient.GetAllAsync(
                cancellationToken);

            var jurisdictionsTask = _jurisdictionsApiClient.GetAllAsync(
                cancellationToken);

            await Task.WhenAll(locationsTask, jurisdictionsTask);

            var locations = locationsTask.Result;
            var jurisdictions = jurisdictionsTask.Result;

            return new LocationsViewModel
            {
                DatosDisponibles = true,
                Ubicaciones = locations
                    .Select(location => new LocationItemViewModel
                    {
                        Id = location.Id,
                        Address = location.Address,
                        Reference = location.Reference,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        JurisdictionName = location.JurisdictionName,
                        CreatedAt = location.CreatedAt
                    })
                    .ToList(),
                OpcionesJurisdiccion = jurisdictions
                    .OrderBy(jurisdiction => jurisdiction.Name)
                    .Select(jurisdiction => new JurisdictionOptionViewModel
                    {
                        Id = jurisdiction.Id,
                        Name = jurisdiction.Name
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir el listado de ubicaciones.");

            return new LocationsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public Task CreateAsync(
        string address,
        string? reference,
        decimal? latitude,
        decimal? longitude,
        int jurisdictionId,
        CancellationToken cancellationToken = default)
    {
        return _locationsApiClient.CreateAsync(
            new CreateLocationRequest
            {
                Address = address,
                Reference = reference,
                Latitude = latitude,
                Longitude = longitude,
                JurisdictionId = jurisdictionId
            },
            cancellationToken);
    }
}
