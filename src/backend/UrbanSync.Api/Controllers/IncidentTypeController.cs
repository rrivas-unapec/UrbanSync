using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.IncidentType;
using UrbanSync.Application.Features.IncidentType;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/incident-types")]
public sealed class IncidentTypesController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,GestorUbicacion,Ciudadano";

    private readonly IIncidentTypeService _service;

    public IncidentTypesController(IIncidentTypeService service)
    {
        _service = service;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<IncidentTypeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentTypeResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var list = await _service.GetAllAsync(cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<IncidentTypeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentTypeResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún tipo de incidencia con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<IncidentTypeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentTypeResponse>> Create(
        [FromBody] CreateIncidentTypeRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(
            new CreateIncidentTypeDto
            {
                Name = request.Name,
                Description = request.Description,
                InstitutionId = request.InstitutionId
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static IncidentTypeResponse MapResponse(IncidentTypeDto dto)
    {
        return new IncidentTypeResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            InstitutionId = dto.InstitutionId,
            InstitutionName = dto.InstitutionName,
            IsActive = dto.IsActive
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