using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Evidence;
using UrbanSync.Application.Features.Evidence;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/evidences")]
public sealed class EvidencesController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,Ciudadano";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,Ciudadano";

    private readonly IEvidenceService _evidenceService;

    public EvidencesController(IEvidenceService evidenceService)
    {
        _evidenceService = evidenceService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("by-incident/{incidentId:int}")]
    [ProducesResponseType<IEnumerable<EvidenceResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EvidenceResponse>>> GetByIncidentId(
        int incidentId,
        CancellationToken cancellationToken)
    {
        var list = await _evidenceService.GetByIncidentIdAsync(incidentId, cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<EvidenceResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EvidenceResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _evidenceService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ninguna evidencia con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<EvidenceResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EvidenceResponse>> Create(
        [FromBody] CreateEvidenceRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _evidenceService.CreateAsync(
            new CreateEvidenceDto
            {
                IncidentId = request.IncidentId,
                EvidenceType = request.EvidenceType,
                FilePath = request.FilePath,
                Description = request.Description,
                UploadedByUserId = request.UploadedByUserId
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static EvidenceResponse MapResponse(EvidenceDto dto)
    {
        return new EvidenceResponse
        {
            Id = dto.Id,
            IncidentId = dto.IncidentId,
            EvidenceType = dto.EvidenceType,
            FilePath = dto.FilePath,
            Description = dto.Description,
            UploadedByUserId = dto.UploadedByUserId,
            UploadedByUserName = dto.UploadedByUserName,
            UploadedAt = dto.UploadedAt
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