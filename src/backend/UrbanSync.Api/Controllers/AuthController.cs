using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Authentication;
using UrbanSync.Api.Contracts.Users;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Application.Features.Roles;
using UrbanSync.Application.Features.Users;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private const string CitizenRoleName = "Ciudadano";

    private readonly IUsuarioService _usuarioService;
    private readonly IRolService _rolService;

    public AuthController(
        IUsuarioService usuarioService,
        IRolService rolService)
    {
        _usuarioService = usuarioService;
        _rolService = rolService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _usuarioService.LoginAsync(
            new LoginRequestDto
            {
                Email = request.Email.Trim(),
                Password = request.Password
            });

        if (result is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciales inválidas",
                Detail =
                    "El correo o la contraseña son incorrectos.",
                Instance = HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] =
                        HttpContext.TraceIdentifier
                }
            });
        }

        return Ok(new LoginResponse
        {
            Token = result.Token,
            ExpiresAtUtc = result.ExpiresAtUtc,
            User = MapUser(result.User)
        });
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<UserResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterRequest request)
    {
        var roles = await _rolService.GetAllAsync();

        var citizenRole = roles.FirstOrDefault(role =>
            string.Equals(
                role.Nombre,
                CitizenRoleName,
                StringComparison.OrdinalIgnoreCase));

        if (citizenRole is null)
        {
            throw new InvalidOperationException(
                "El rol Ciudadano no está configurado.");
        }

        var normalizedEmail = request.Email.Trim();

        var createdUser = await _usuarioService.CreateAsync(
            new UsuarioCreateDto
            {
                NombreUsuario = normalizedEmail,
                NombreCompleto =
                    request.NombreCompleto.Trim(),
                Email = normalizedEmail,
                Password = request.Password,
                RolId = citizenRole.Id
            });

        var response = MapUser(createdUser);

        return Created(
            $"/api/usuarios/{response.Id}",
            response);
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
}