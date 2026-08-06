using Microsoft.AspNetCore.Mvc;
using UrbanSync.Business.Services;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Api.Controllers
{
    public class IncidenciaController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public class IncidenciasController : ControllerBase
        {
            private readonly IIncidenciaService _incidenciaService;

            public IncidenciasController(IIncidenciaService incidenciaService)
            {
                _incidenciaService = incidenciaService;
            }

            [HttpGet]
            public async Task<IActionResult> GetAll()
            {
                var result = await _incidenciaService.GetAllIncidenciasAsync();
                return Ok(result);
            }

            [HttpGet("{id:int}")]
            public async Task<IActionResult> GetById(int id)
            {
                var result = await _incidenciaService.GetIncidenciaByIdAsync(id);
                if (result == null) return NotFound(new { mensaje = "Incidencia no encontrada" });
                return Ok(result);
            }

            [HttpPost]
            public async Task<IActionResult> Create([FromBody] IncidenciaCreateDto dto)
            {
                var created = await _incidenciaService.CreateIncidenciaAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }

            [HttpPatch("{id:int}/estado")]
            public async Task<IActionResult> UpdateEstado(int id, [FromBody] IncidenciaEstadoUpdateDto dto)
            {
                var success = await _incidenciaService.UpdateEstadoAsync(id, dto);
                if (!success) return NotFound(new { mensaje = "Incidencia no encontrada o no se pudo actualizar" });
                return NoContent();
            }
        }
    }
}
