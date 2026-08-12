using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.TechnicalAnalysis;
using UrbanSync.Application.Features.TechnicalAnalysis;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/technical-analyses")]
public sealed class TechnicalAnalysisController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly ITechnicalAnalysisService _technicalAnalysisService;

    public TechnicalAnalysisController(ITechnicalAnalysisService technicalAnalysisService)
    {
        _technicalAnalysisService = technicalAnalysisService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("by-incident/{incidentId:int}")]
    [ProducesResponseType<TechnicalAnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicalAnalysisResponse>> GetByIncidentId(
        int incidentId,
        CancellationToken cancellationToken)
    {
        var item = await _technicalAnalysisService.GetByIncidentIdAsync(incidentId, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún análisis técnico para la incidencia {incidentId}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<TechnicalAnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnicalAnalysisResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _technicalAnalysisService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún análisis técnico con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<TechnicalAnalysisResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TechnicalAnalysisResponse>> Create(
        [FromBody] CreateTechnicalAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _technicalAnalysisService.CreateAsync(
            new CreateTechnicalAnalysisDto
            {
                IncidentId = request.IncidentId,
                TechnicalUserId = request.TechnicalUserId,
                Diagnosis = request.Diagnosis,
                RecommendedActions = request.RecommendedActions
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static TechnicalAnalysisResponse MapResponse(TechnicalAnalysisDto dto)
    {
        return new TechnicalAnalysisResponse
        {
            Id = dto.Id,
            IncidentId = dto.IncidentId,
            TechnicalUserId = dto.TechnicalUserId,
            TechnicalUserName = dto.TechnicalUserName,
            Diagnosis = dto.Diagnosis,
            RecommendedActions = dto.RecommendedActions,
            AnalysisDate = dto.AnalysisDate
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