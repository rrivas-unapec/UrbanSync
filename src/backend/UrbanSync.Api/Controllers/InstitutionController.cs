using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Institutions;
using UrbanSync.Application.Features.Institution;

namespace UrbanSync.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/institutions")]
public sealed class InstitutionsController : ControllerBase
{
    private const string WriteRoles = "Administrador,SupervisorOperaciones";
    private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,Ciudadano";

    private readonly IInstitutionService _institutionService;

    public InstitutionsController(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet]
    [ProducesResponseType<IEnumerable<InstitutionResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InstitutionResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var list = await _institutionService.GetAllAsync(cancellationToken);
        return Ok(list.Select(MapResponse));
    }

    [Authorize(Roles = ReadRoles)]
    [HttpGet("{id:int}")]
    [ProducesResponseType<InstitutionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InstitutionResponse>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var item = await _institutionService.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(
                CreateNotFoundProblem($"No se encontró ninguna institución con el ID {id}."));
        }

        return Ok(MapResponse(item));
    }

    [Authorize(Roles = WriteRoles)]
    [HttpPost]
    [ProducesResponseType<InstitutionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InstitutionResponse>> Create(
        [FromBody] CreateInstitutionRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _institutionService.CreateAsync(
            new CreateInstitutionDto
            {
                Name = request.Name,
                InstitutionType = request.InstitutionType,
                ContactEmail = request.ContactEmail,
                ContactPhone = request.ContactPhone
            },
            cancellationToken);

        var response = MapResponse(created);

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    private static InstitutionResponse MapResponse(InstitutionDto dto)
    {
        return new InstitutionResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            InstitutionType = dto.InstitutionType,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            IsActive = dto.IsActive
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