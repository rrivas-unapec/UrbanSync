using UrbanSync.Web.ApiClients.Jurisdictions;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Jurisdictions;

public sealed class JurisdictionsPageService : IJurisdictionsPageService
{
    private readonly IJurisdictionsApiClient _jurisdictionsApiClient;
    private readonly ILogger<JurisdictionsPageService> _logger;

    public JurisdictionsPageService(
        IJurisdictionsApiClient jurisdictionsApiClient,
        ILogger<JurisdictionsPageService> logger)
    {
        _jurisdictionsApiClient = jurisdictionsApiClient;
        _logger = logger;
    }

    public async Task<JurisdictionsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var jurisdictions = await _jurisdictionsApiClient.GetAllAsync(
                cancellationToken);

            return new JurisdictionsViewModel
            {
                DatosDisponibles = true,
                Jurisdicciones = jurisdictions
                    .Select(jurisdiction => new JurisdictionItemViewModel
                    {
                        Id = jurisdiction.Id,
                        Name = jurisdiction.Name,
                        Level = jurisdiction.Level,
                        ParentJurisdictionName = jurisdiction.ParentJurisdictionName,
                        IsActive = jurisdiction.IsActive
                    })
                    .ToList(),
                OpcionesPadre = jurisdictions
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
                "No se pudo construir el listado de jurisdicciones.");

            return new JurisdictionsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public Task CreateAsync(
        string name,
        string level,
        int? parentJurisdictionId,
        CancellationToken cancellationToken = default)
    {
        return _jurisdictionsApiClient.CreateAsync(
            new CreateJurisdictionRequest
            {
                Name = name,
                Level = level,
                ParentJurisdictionId = parentJurisdictionId
            },
            cancellationToken);
    }
}
