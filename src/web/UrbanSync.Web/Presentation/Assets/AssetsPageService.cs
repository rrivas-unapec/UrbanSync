using UrbanSync.Web.ApiClients.Assets;
using UrbanSync.Web.ApiClients.Jurisdictions;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Assets;

public sealed class AssetsPageService : IAssetsPageService
{
    private readonly IAssetsApiClient _assetsApiClient;
    private readonly IJurisdictionsApiClient _jurisdictionsApiClient;
    private readonly ILogger<AssetsPageService> _logger;

    public AssetsPageService(
        IAssetsApiClient assetsApiClient,
        IJurisdictionsApiClient jurisdictionsApiClient,
        ILogger<AssetsPageService> logger)
    {
        _assetsApiClient = assetsApiClient;
        _jurisdictionsApiClient = jurisdictionsApiClient;
        _logger = logger;
    }

    public async Task<AssetsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var assetsTask = _assetsApiClient.GetAllAsync(
                cancellationToken);

            var jurisdictionsTask = _jurisdictionsApiClient.GetAllAsync(
                cancellationToken);

            await Task.WhenAll(assetsTask, jurisdictionsTask);

            var assets = assetsTask.Result;
            var jurisdictions = jurisdictionsTask.Result;

            return new AssetsViewModel
            {
                DatosDisponibles = true,
                Activos = assets
                    .Select(ToItemViewModel)
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
                "No se pudo construir el listado de activos urbanos.");

            return new AssetsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public async Task<AssetDetailsViewModel?> BuildDetailsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetsApiClient.GetByIdAsync(
            id,
            cancellationToken);

        if (asset is null)
        {
            return null;
        }

        var model = new AssetDetailsViewModel
        {
            Asset = ToItemViewModel(asset)
        };

        try
        {
            var history = await _assetsApiClient.GetHistoryAsync(
                id,
                cancellationToken);

            model.Historial = history?
                .Select(item => new AssetHistoryItemViewModel
                {
                    IncidentId = item.IncidentId,
                    CaseCode = item.CaseCode,
                    IncidentType = item.IncidentType,
                    Description = item.Description,
                    Status = item.Status,
                    ReportDate = item.ReportDate
                })
                .ToList() ?? [];
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar el historial del activo {AssetId}.",
                id);

            model.HistorialDisponible = false;
        }

        return model;
    }

    public Task CreateAsync(
        string code,
        string name,
        string type,
        string status,
        int jurisdictionId,
        DateTime? installationDate,
        CancellationToken cancellationToken = default)
    {
        return _assetsApiClient.CreateAsync(
            new CreateAssetRequest
            {
                Code = code,
                Name = name,
                Type = type,
                Status = status,
                JurisdictionId = jurisdictionId,
                InstallationDate = installationDate
            },
            cancellationToken);
    }

    private static AssetItemViewModel ToItemViewModel(
        AssetResponse asset)
    {
        return new AssetItemViewModel
        {
            Id = asset.Id,
            Code = asset.Code,
            Name = asset.Name,
            Type = asset.Type,
            Status = asset.Status,
            JurisdictionName = asset.JurisdictionName,
            InstallationDate = asset.InstallationDate,
            IsActive = asset.IsActive
        };
    }
}
