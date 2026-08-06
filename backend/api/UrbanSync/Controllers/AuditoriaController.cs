using Microsoft.AspNetCore.Mvc;
using UrbanSync.Business.Services;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Route("api/activity")]
public class ActivityController : ControllerBase
{
    private readonly IAuditoriaService _auditoriaService;

    public ActivityController(IAuditoriaService auditoriaService)
    {
        _auditoriaService = auditoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] AuditoriaFilterDto filter)
    {
        var logs = await _auditoriaService.GetLogsAsync(filter);
        return Ok(logs);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetLogById(long id)
    {
        var log = await _auditoriaService.GetLogByIdAsync(id);
        if (log == null) return NotFound(new { mensaje = "Registro de auditoría no encontrado" });
        return Ok(log);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterLog([FromBody] AuditoriaCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IpOrigen))
        {
            dto.IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        var createdLog = await _auditoriaService.RegisterLogAsync(dto);
        return CreatedAtAction(nameof(GetLogById), new { id = createdLog.Id }, createdLog);
    }
}