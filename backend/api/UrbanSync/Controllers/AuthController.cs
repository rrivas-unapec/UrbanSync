using Microsoft.AspNetCore.Mvc;
using UrbanSync.Business.Services;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Controllers
{
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
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            var response = await _usuarioService.LoginAsync(dto);

            if (response is null)
                return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

            return Ok(response);
        }
    }
}
