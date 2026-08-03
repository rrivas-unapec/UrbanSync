using Microsoft.AspNetCore.Mvc;
using UrbanSync.Application.Features.Authentication;
using UrbanSync.Application.Features.Users;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public AuthController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto dto)
    {
        var response = await _usuarioService.LoginAsync(dto);

        if (response is null)
        {
            return Unauthorized(new
            {
                mensaje = "Usuario o contraseña incorrectos."
            });
        }

        return Ok(response);
    }
}