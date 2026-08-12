using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Report;
using UrbanSync.Application.Features.Report;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("by-incident/{incidentId:int}")]
    [ProducesResponseType<IEnumerable<ReportResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportResponse>>> GetByIncidentId(
        int incidentId,
        CancellationToken cancellationToken)
    {
        var list = await _reportService.GetByIncidentIdAsync(incidentId, cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _reportService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún reporte con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<ReportResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportResponse>> Create(
        [FromBody] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _reportService.CreateAsync(
            new CreateReportDto
            {
                IncidentId = request.IncidentId,
                JobId = request.JobId,
                GeneratedByUserId = request.GeneratedByUserId,
                Content = request.Content,
                FilePath = request.FilePath
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static ReportResponse MapResponse(ReportDto dto)
    {
        return new ReportResponse
        {
            Id = dto.Id,
            IncidentId = dto.IncidentId,
            JobId = dto.JobId,
            GeneratedByUserId = dto.GeneratedByUserId,
            GeneratedByUserName = dto.GeneratedByUserName,
            Content = dto.Content,
            FilePath = dto.FilePath,
            GeneratedAt = dto.GeneratedAt
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