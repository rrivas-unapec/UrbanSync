using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Claim;
using UrbanSync.Application.Features.Claim;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/claims")]
public sealed class ClaimsController : ControllerBase
{
    private const string AdminRoles = "Administrador,SupervisorOperaciones";
    private const string CitizenRoles = "Administrador,SupervisorOperaciones,Ciudadano";

    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService)
    {
        _claimService = claimService;
    }

    [Authorize(Roles = AdminRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<ClaimResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var list = await _claimService.GetAllAsync(cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = CitizenRoles)]
    [HttpGet("my-claims/{citizenUserId:int}")]
    [ProducesResponseType<IEnumerable<ClaimResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetByCitizenId(
        int citizenUserId,
        CancellationToken cancellationToken)
    {
        var list = await _claimService.GetByCitizenIdAsync(citizenUserId, cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = CitizenRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClaimResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _claimService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ninguna reclamación con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = CitizenRoles)]
    [HttpPost]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> Create(
        [FromBody] CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _claimService.CreateAsync(
            new CreateClaimDto
            {
                CitizenUserId = request.CitizenUserId,
                LocationId = request.LocationId,
                Category = request.Category,
                Title = request.Title,
                Description = request.Description
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [Authorize(Roles = AdminRoles)]
    [HttpPut("{id:int}/status")]
    [ProducesResponseType<ClaimResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> UpdateStatus(
        int id,
        [FromBody] UpdateClaimStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _claimService.UpdateStatusAsync(
            new UpdateClaimStatusDto
            {
                Id = id,
                Status = request.Status
            },
            cancellationToken);

        if (updated is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ninguna reclamación con el ID {id} para actualizar."));
        }

        return Ok(MapResponse(updated));
    }

    private static ClaimResponse MapResponse(ClaimDto dto)
    {
        return new ClaimResponse
        {
            Id = dto.Id,
            CitizenUserId = dto.CitizenUserId,
            CitizenUserName = dto.CitizenUserName,
            LocationId = dto.LocationId,
            LocationAddress = dto.LocationAddress,
            Category = dto.Category,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            CreatedAt = dto.CreatedAt
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