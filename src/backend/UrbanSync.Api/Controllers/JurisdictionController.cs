using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Api.Contracts.Jurisdiction;
using UrbanSync.Application.Features.Jurisdiction;

namespace UrbanSync.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/jurisdictions")]
    public sealed class JurisdictionsController : ControllerBase
    {
        private const string WriteRoles = "Administrador,SupervisorOperaciones";
        private const string ReadRoles = "Administrador,SupervisorOperaciones,AnalistaTecnico,GestorUbicacion";

        private readonly IJurisdictionService _jurisdictionService;

        public JurisdictionsController(IJurisdictionService jurisdictionService)
        {
            _jurisdictionService = jurisdictionService;
        }

        [Authorize(Roles = ReadRoles)]
        [HttpGet]
        [ProducesResponseType<IEnumerable<JurisdictionResponse>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JurisdictionResponse>>> GetAll(
            CancellationToken cancellationToken)
        {
            var list = await _jurisdictionService.GetAllAsync(cancellationToken);
            return Ok(list.Select(MapResponse));
        }

        [Authorize(Roles = ReadRoles)]
        [HttpGet("{id:int}")]
        [ProducesResponseType<JurisdictionResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JurisdictionResponse>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var item = await _jurisdictionService.GetByIdAsync(id, cancellationToken);

            if (item is null)
            {
                return NotFound(
                    CreateNotFoundProblem($"No se encontró ninguna jurisdicción con el ID {id}."));
            }

            return Ok(MapResponse(item));
        }

        [Authorize(Roles = WriteRoles)]
        [HttpPost]
        [ProducesResponseType<JurisdictionResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JurisdictionResponse>> Create(
            [FromBody] CreateJurisdictionRequest request,
            CancellationToken cancellationToken)
        {
            var created = await _jurisdictionService.CreateAsync(
                new CreateJurisdictionDto
                {
                    Name = request.Name,
                    Level = request.Level,
                    ParentJurisdictionId = request.ParentJurisdictionId
                },
                cancellationToken);

            var response = MapResponse(created);

            return CreatedAtAction(
                nameof(GetById),
                new { id = response.Id },
                response);
        }

        private static JurisdictionResponse MapResponse(JurisdictionDto dto)
        {
            return new JurisdictionResponse
            {
                Id = dto.Id,
                Name = dto.Name,
                Level = dto.Level,
                ParentJurisdictionId = dto.ParentJurisdictionId,
                ParentJurisdictionName = dto.ParentJurisdictionName,
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
}