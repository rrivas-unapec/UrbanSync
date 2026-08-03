using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Users;
using UrbanSync.Application.Features.Users;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<UserResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll()
    {
        var users = await _usuarioService.GetAllAsync();

        return Ok(users.Select(MapUser));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<UserResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(int id)
    {
        var user = await _usuarioService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ningún usuario con el ID {id}."));
        }

        return Ok(MapUser(user));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request)
    {
        var createdUser = await _usuarioService.CreateAsync(
            new UsuarioCreateDto
            {
                NombreUsuario = request.NombreUsuario.Trim(),
                NombreCompleto = request.NombreCompleto.Trim(),
                Email = request.Email.Trim(),
                Password = request.Password,
                RolId = request.RolId
            });

        var response = MapUser(createdUser);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [HttpPatch("{id:int}/toggle-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var changed = await _usuarioService.ToggleStatusAsync(id);

        if (!changed)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ningún usuario con el ID {id}."));
        }

        return NoContent();
    }

    private static UserResponse MapUser(UsuarioDto user)
    {
        return new UserResponse
        {
            Id = user.Id,
            NombreUsuario = user.NombreUsuario,
            NombreCompleto = user.NombreCompleto,
            Email = user.Email,
            RolId = user.RolId,
            RolNombre = user.RolNombre,
            Activo = user.Activo
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