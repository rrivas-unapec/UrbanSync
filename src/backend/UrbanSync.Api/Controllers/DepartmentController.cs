using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Departaments;
using UrbanSync.Api.Contracts.Department;
using UrbanSync.Application.Features.Department;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,GestorUbicacion";

    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<DepartmentResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DepartmentResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var list = await _departmentService.GetAllAsync(cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _departmentService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún departamento con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<DepartmentResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentResponse>> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _departmentService.CreateAsync(
            new CreateDepartmentDto
            {
                Name = request.Name,
                JurisdictionId = request.JurisdictionId
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static DepartmentResponse MapResponse(DepartmentDto dto)
    {
        return new DepartmentResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            JurisdictionId = dto.JurisdictionId,
            JurisdictionName = dto.JurisdictionName,
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