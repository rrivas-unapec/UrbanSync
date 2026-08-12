using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Location;
using UrbanSync.Application.Features.Location;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/locations")]
public sealed class LocationsController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones,Ciudadano,AnalistaTecnico";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,Ciudadano";

    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<LocationResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<LocationResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var list = await _locationService.GetAllAsync(cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<LocationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LocationResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _locationService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ninguna ubicación con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<LocationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LocationResponse>> Create(
        [FromBody] CreateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _locationService.CreateAsync(
            new CreateLocationDto
            {
                Address = request.Address,
                Reference = request.Reference,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                JurisdictionId = request.JurisdictionId
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static LocationResponse MapResponse(LocationDto dto)
    {
        return new LocationResponse
        {
            Id = dto.Id,
            Address = dto.Address,
            Reference = dto.Reference,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            JurisdictionId = dto.JurisdictionId,
            JurisdictionName = dto.JurisdictionName,
            CreatedAt = dto.CreatedAt
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