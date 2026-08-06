using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Roles;
using UrbanSync.Application.Features.Roles;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<RoleResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAll()
    {
        var roles = await _rolService.GetAllAsync();

        return Ok(roles.Select(MapRole));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<RoleResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetById(int id)
    {
        var role = await _rolService.GetByIdAsync(id);

        if (role is null)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ningún rol con el ID {id}."));
        }

        return Ok(MapRole(role));
    }

    [HttpPost]
    [ProducesResponseType<RoleResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleResponse>> Create(
        [FromBody] CreateRoleRequest request)
    {
        var createdRole = await _rolService.CreateAsync(
            new RolCreateDto
            {
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim()
            });

        var response = MapRole(createdRole);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static RoleResponse MapRole(RolDto role)
    {
        return new RoleResponse
        {
            Id = role.Id,
            Nombre = role.Nombre,
            Descripcion = role.Descripcion
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