using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Audit;
using UrbanSync.Application.Features.Audit;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/activity")]
public sealed class ActivityController : ControllerBase
{
    private const string AuditReadRoles =
        "Administrador,SupervisorOperaciones";

    private readonly IAuditService _auditService;

    public ActivityController(
        IAuditService auditService)
    {
        _auditService = auditService;
    }

    [Authorize(Roles = AuditReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<AuditResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AuditResponse>>> GetAll(
        [FromQuery] int? usuarioId,
        [FromQuery] string? entidad,
        [FromQuery] string? accion,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin,
        CancellationToken cancellationToken)
    {
        var audits = await _auditService.GetAllAsync(
            new AuditFilterDto
            {
                UserId = usuarioId,
                Entity = entidad,
                Action = accion,
                StartDate = fechaInicio,
                EndDate = fechaFin
            },
            cancellationToken);

        return Ok(
            audits.Select(MapAudit));
    }

    [Authorize(Roles = AuditReadRoles)]
    [HttpGet("{id:long}")]
    [ProducesResponseType<AuditResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var audit = await _auditService.GetByIdAsync(
            id,
            cancellationToken);

        if (audit is null)
        {
            return NotFound(
                CreateNotFoundProblem(
                    $"No se encontró ningún registro de auditoría con el ID {id}."));
        }

        return Ok(MapAudit(audit));
    }

    [HttpPost]
    [ProducesResponseType<AuditResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuditResponse>> Create(
        [FromBody] CreateAuditRequest request,
        CancellationToken cancellationToken)
    {
        var createdAudit = await _auditService.CreateAsync(
            new CreateAuditDto
            {
                UserId = GetAuthenticatedUserId(),
                Action = request.Accion,
                Entity = request.Entidad,
                EntityId = request.EntidadId,
                Detail = request.Detalle,
                IpAddress =
                    HttpContext.Connection
                        .RemoteIpAddress?
                        .ToString()
            },
            cancellationToken);

        var response = MapAudit(createdAudit);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private int GetAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(
                userIdValue,
                out var userId) ||
            userId <= 0)
        {
            throw new InvalidOperationException(
                "No fue posible identificar al usuario autenticado.");
        }

        return userId;
    }

    private static AuditResponse MapAudit(
        AuditDto audit)
    {
        return new AuditResponse
        {
            Id = audit.Id,
            UsuarioId = audit.UserId,
            NombreUsuario = audit.UserName,
            Accion = audit.Action,
            Entidad = audit.Entity,
            EntidadId = audit.EntityId,
            Detalle = audit.Detail,
            IpOrigen = audit.IpAddress,
            FechaHora = audit.Timestamp
        };
    }

    private ProblemDetails CreateNotFoundProblem(
        string detail)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso no encontrado",
            Detail = detail,
            Instance = HttpContext.Request.Path,
            Extensions =
            {
                ["traceId"] =
                    HttpContext.TraceIdentifier
            }
        };
    }
}