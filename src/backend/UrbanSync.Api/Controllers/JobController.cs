using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Job;
using UrbanSync.Application.Features.Job;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/jobs")]
public sealed class JobsController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("by-incident/{incidentId:int}")]
    [ProducesResponseType<IEnumerable<JobResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobResponse>>> GetByIncidentId(
        int incidentId,
        CancellationToken cancellationToken)
    {
        var list = await _jobService.GetByIncidentIdAsync(incidentId, cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<JobResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _jobService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún trabajo con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<JobResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobResponse>> Create(
        [FromBody] CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _jobService.CreateAsync(
            new CreateJobDto
            {
                IncidentId = request.IncidentId,
                AssignedUserId = request.AssignedUserId,
                JobDescription = request.JobDescription,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Result = request.Result
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPut("{id:int}")]
    [ProducesResponseType<JobResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<JobResponse>> Update(
        int id,
        [FromBody] UpdateJobRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _jobService.UpdateAsync(
            new UpdateJobDto
            {
                Id = id,
                Status = request.Status,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Result = request.Result
            },
            cancellationToken);

        if (updated is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ningún trabajo con el ID {id} para actualizar."));
        }

        return Ok(MapResponse(updated));
    }

    private static JobResponse MapResponse(JobDto dto)
    {
        return new JobResponse
        {
            Id = dto.Id,
            IncidentId = dto.IncidentId,
            AssignedUserId = dto.AssignedUserId,
            AssignedUserName = dto.AssignedUserName,
            JobDescription = dto.JobDescription,
            Status = dto.Status,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Result = dto.Result
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