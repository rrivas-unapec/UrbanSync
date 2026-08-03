using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Authentication;
using UrbanSync.Api.Contracts.Users;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Application.Features.Users;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

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
                Email = request.Email,
                Password = request.Password
            });

        if (result is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciales inválidas",
                Detail = "El correo o la contraseña son incorrectos.",
                Instance = HttpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = HttpContext.TraceIdentifier
                }
            });
        }

        return Ok(new LoginResponse
        {
            Token = result.Token,
            User = MapUser(result.User)
        });
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