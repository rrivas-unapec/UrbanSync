using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Incidents;
using UrbanSync.Application.Features.Incidents;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/incidents")]
public sealed class IncidentsController : ControllerBase
{
    private const string ManagementRoles =
        "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly IIncidentService _incidentService;

    public IncidentsController(
        IIncidentService incidentService)
    {
        _incidentService = incidentService;
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<IncidentResponse>>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<IncidentResponse>>> GetAll(
        [FromQuery] string? status,
        [FromQuery] bool mine = false,
        CancellationToken cancellationToken = default)
    {
        int? reportingUserId = null;

        if (mine)
        {
            reportingUserId = GetAuthenticatedUserId();
        }

        var incidents =
            await _incidentService.GetAllAsync(
                status,
                reportingUserId,
                cancellationToken);

        return Ok(incidents.Select(MapIncident));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<IncidentResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var incident =
            await _incidentService.GetByIdAsync(
                id,
                cancellationToken);

        if (incident is null)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ninguna incidencia con el ID {id}."));
        }

        return Ok(MapIncident(incident));
    }

    [HttpPost]
    [ProducesResponseType<IncidentResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IncidentResponse>> Create(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        var createdIncident =
            await _incidentService.CreateAsync(
                new CreateIncidentDto
                {
                    TipoIncidenciaId =
                        request.TipoIncidenciaId,

                    ActivoId =
                        request.ActivoId,

                    Descripcion =
                        request.Descripcion.Trim(),

                    Prioridad =
                        request.Prioridad.Trim(),

                    Direccion =
                        request.Ubicacion.Direccion.Trim(),

                    Referencia =
                        request.Ubicacion.Referencia?.Trim(),

                    Latitud =
                        request.Ubicacion.Lat,

                    Longitud =
                        request.Ubicacion.Lng,

                    JurisdiccionId =
                        request.Ubicacion.JurisdiccionId
                        ?? throw new ArgumentException(
                            "La jurisdicción es obligatoria.")
                },
                userId,
                cancellationToken);

        var response = MapIncident(createdIncident);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<IncidentResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> UpdateStatus(
        int id,
        [FromBody] UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updatedIncident =
            await _incidentService.UpdateStatusAsync(
                id,
                new UpdateIncidentStatusDto
                {
                    Estado = request.Estado.Trim(),
                    InstitucionAsignadaId =
                        request.InstitucionAsignadaId
                },
                GetAuthenticatedUserId(),
                cancellationToken);

        if (updatedIncident is null)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ninguna incidencia con el ID {id}."));
        }

        return Ok(MapIncident(updatedIncident));
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPatch("{id:int}/triage")]
    [ProducesResponseType<IncidentResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IncidentResponse>> Triage(
        int id,
        [FromBody] TriageIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var updatedIncident =
            await _incidentService.TriageAsync(
                id,
                new TriageIncidentDto
                {
                    TipoIncidenciaId =
                        request.TipoIncidenciaId,

                    Prioridad =
                        request.Prioridad?.Trim(),

                    Accion =
                        request.Accion?.Trim(),

                    JurisdiccionId =
                        request.JurisdiccionId
                },
                GetAuthenticatedUserId(),
                cancellationToken);

        if (updatedIncident is null)
        {
            return NotFound(CreateNotFoundProblem(
                $"No se encontró ninguna incidencia con el ID {id}."));
        }

        return Ok(MapIncident(updatedIncident));
    }

    private int GetAuthenticatedUserId()
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId) ||
            userId <= 0)
        {
            throw new InvalidOperationException(
                "No fue posible identificar al usuario autenticado.");
        }

        return userId;
    }

    private static IncidentResponse MapIncident(
        IncidentDto incident)
    {
        return new IncidentResponse
        {
            Id = incident.Id,
            CodigoCaso = incident.CodigoCaso,
            Estado = incident.Estado,
            Prioridad = incident.Prioridad,
            Descripcion = incident.Descripcion,
            TipoIncidenciaId =
                incident.TipoIncidenciaId,
            TipoIncidencia =
                incident.TipoIncidencia,
            ActivoId =
                incident.ActivoId,
            JurisdiccionId =
                incident.JurisdiccionId,
            Jurisdiccion =
                incident.Jurisdiccion,
            Direccion =
                incident.Direccion,
            UsuarioReporta =
                incident.UsuarioReporta,
            FechaReporte =
                incident.FechaReporte,
            InstitucionAsignadaId =
                incident.InstitucionAsignadaId,
            InstitucionAsignada =
                incident.InstitucionAsignada,
            Referencia =
                incident.Referencia,
            Latitud =
                incident.Latitud,
            Longitud =
                incident.Longitud,
            FechaAsignacion =
                incident.FechaAsignacion,
            FechaCierre =
                incident.FechaCierre
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