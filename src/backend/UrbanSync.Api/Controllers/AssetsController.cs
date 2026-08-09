using UrbanSync.Api.Contracts.Assets;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/assets")]
public sealed class AssetsController : ControllerBase
{
    private const string AssetWriteRoles = "Administrador,SupervisorOperaciones";
    private const string AssetReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,GestorUbicacion";

    private readonly IAssetService _assetService;

    public AssetsController(IAssetService assetService)
    {
        _assetService = assetService;
    }

    [Authorize(Roles = AssetReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<AssetResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AssetResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var assets = await _assetService.GetAllAsync(cancellationToken);
        return Ok(assets.Select(MapAsset));
    }

    [Authorize(Roles = AssetReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<AssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var asset = await _assetService.GetByIdAsync(id, cancellationToken);

        if (asset is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún activo urbano con el ID {id}."));
        }

        return Ok(MapAsset(asset));
    }

    [Authorize(Roles = AssetReadRoles)]
    [HttpGet("{id:int}/history")]
    [ProducesResponseType<AssetHistoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetHistoryResponse>> GetHistory(
        int id,
        CancellationToken cancellationToken)
    {
        var history = await _assetService.GetHistoryByIdAsync(id, cancellationToken);

        if (history is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún historial asociado al activo con ID {id}."));
        }

        return Ok(MapHistory(history));
    }

    [Authorize(Roles = AssetWriteRoles)]
    [HttpPost]
    [ProducesResponseType<AssetResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AssetResponse>> Create(
        [FromBody] CreateAssetRequest request,
        CancellationToken cancellationToken)
    {
        var createdAsset = await _assetService.CreateAsync(
            new CreateAssetDto
            {
                Code = request.Code,
                Name = request.Name,
                Type = request.Type,
                Status = request.Status,
                JurisdictionId = request.JurisdictionId,
                InstallationDate = request.InstallationDate
            },
            cancellationToken);

        var response = MapAsset(createdAsset);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static AssetResponse MapAsset(AssetDto asset)
    {
        return new AssetResponse
        {
            Id = asset.Id,
            Code = asset.Code,
            Name = asset.Name,
            Type = asset.Type,
            Status = asset.Status,
            JurisdictionId = asset.JurisdictionId,
            JurisdictionName = asset.JurisdictionName,
            InstallationDate = asset.InstallationDate,
            IsActive = asset.IsActive
        };
    }

    private static AssetHistoryResponse MapHistory(AssetHistoryDto history)
    {
        return new AssetHistoryResponse
        {
            IncidentId = history.IncidentId,
            CaseCode = history.CaseCode,
            IncidentType = history.IncidentType,
            Description = history.Description,
            Status = history.Status,
            ReportDate = history.ReportDate
        };
    }

    private ProblemDetails CreateNotFoundProblem(string detail)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso no encontrado",
            Detail = detail,
            Instance = HttpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = HttpContext.TraceIdentifier
            }
        };
    }
}