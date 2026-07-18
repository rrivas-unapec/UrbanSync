using Microsoft.AspNetCore.Mvc;
using UrbanSync.Application.Services;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRolService _rolService;

        public RolesController(IRolService rolService)
        {
            _rolService = rolService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _rolService.GetAllAsync();
            return Ok(roles);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rol = await _rolService.GetByIdAsync(id);
            return rol is null ? NotFound() : Ok(rol);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RolCreateDto dto)
        {
            try
            {
                var creado = await _rolService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = creado.Id }, creado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
