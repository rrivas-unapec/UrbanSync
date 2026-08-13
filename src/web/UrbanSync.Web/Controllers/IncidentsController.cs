using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Evidence;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.TechnicalAnalysis;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class IncidentsController : Controller
{
    private const string ManagementRoles =
        "Administrador,SupervisorOperaciones,AnalistaTecnico";

    private readonly IIncidentsApiClient
        _incidentsApiClient;

    private readonly IEvidenceApiClient
        _evidenceApiClient;

    private readonly ITechnicalAnalysisApiClient
        _technicalAnalysisApiClient;

    private readonly ILogger<IncidentsController>
        _logger;

    public IncidentsController(
        IIncidentsApiClient incidentsApiClient,
        IEvidenceApiClient evidenceApiClient,
        ITechnicalAnalysisApiClient technicalAnalysisApiClient,
        ILogger<IncidentsController> logger)
    {
        _incidentsApiClient = incidentsApiClient;
        _evidenceApiClient = evidenceApiClient;
        _technicalAnalysisApiClient = technicalAnalysisApiClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? status,
        CancellationToken cancellationToken)
    {
        var canManage =
            User.IsInRole("Administrador") ||
            User.IsInRole("SupervisorOperaciones") ||
            User.IsInRole("AnalistaTecnico");

        ViewBag.Status = status;
        ViewBag.MineOnly = !canManage;

        try
        {
            var incidents =
                await _incidentsApiClient.GetAllAsync(
                    status,
                    mine: !canManage,
                    cancellationToken);

            return View(incidents);
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron consultar las incidencias.");

            ViewBag.Error = exception.Message;

            return View(
                Array.Empty<IncidentResponse>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        int id,
        CancellationToken cancellationToken)
    {
        IncidentResponse? incident;

        try
        {
            incident =
                await _incidentsApiClient.GetByIdAsync(
                    id,
                    cancellationToken);

            if (incident is null)
            {
                return NotFound();
            }
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar la incidencia {IncidentId}.",
                id);

            TempData["IncidentError"] =
                exception.Message;

            return RedirectToAction(
                nameof(Index));
        }

        var model = new IncidentDetailsViewModel
        {
            Incident = incident
        };

        try
        {
            var evidences =
                await _evidenceApiClient.GetByIncidentIdAsync(
                    id,
                    cancellationToken);

            model.Evidencias = evidences
                .Select(evidence => new EvidenceItemViewModel
                {
                    Id = evidence.Id,
                    EvidenceType = evidence.EvidenceType,
                    FilePath = evidence.FilePath,
                    Description = evidence.Description,
                    UploadedByUserName = evidence.UploadedByUserName,
                    UploadedAt = evidence.UploadedAt
                })
                .ToList();
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron consultar las evidencias de la incidencia {IncidentId}.",
                id);

            model.EvidenciasDisponibles = false;
        }

        try
        {
            var analysis =
                await _technicalAnalysisApiClient.GetByIncidentIdAsync(
                    id,
                    cancellationToken);

            model.AnalisisTecnico = analysis is null
                ? null
                : new TechnicalAnalysisItemViewModel
                {
                    Id = analysis.Id,
                    TechnicalUserName = analysis.TechnicalUserName,
                    Diagnosis = analysis.Diagnosis,
                    RecommendedActions = analysis.RecommendedActions,
                    AnalysisDate = analysis.AnalysisDate
                };
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar el análisis técnico de la incidencia {IncidentId}.",
                id);

            model.AnalisisTecnicoDisponible = false;
        }

        return View(model);
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadEvidence(
        int incidentId,
        string evidenceType,
        string filePath,
        string? description,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(evidenceType) ||
            string.IsNullOrWhiteSpace(filePath))
        {
            TempData["IncidentError"] =
                "Debes indicar el tipo de evidencia y la ruta del archivo.";

            return RedirectToAction(
                nameof(Details),
                new { id = incidentId });
        }

        try
        {
            await _evidenceApiClient.CreateAsync(
                new CreateEvidenceRequest
                {
                    IncidentId = incidentId,
                    EvidenceType = evidenceType,
                    FilePath = filePath,
                    Description = description,
                    UploadedByUserId = GetAuthenticatedUserId()
                },
                cancellationToken);

            TempData["IncidentSuccess"] =
                "La evidencia fue registrada correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo registrar la evidencia de la incidencia {IncidentId}.",
                incidentId);

            TempData["IncidentError"] =
                exception.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = incidentId });
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTechnicalAnalysis(
        int incidentId,
        string diagnosis,
        string? recommendedActions,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(diagnosis))
        {
            TempData["IncidentError"] =
                "El diagnóstico es obligatorio.";

            return RedirectToAction(
                nameof(Details),
                new { id = incidentId });
        }

        try
        {
            await _technicalAnalysisApiClient.CreateAsync(
                new CreateTechnicalAnalysisRequest
                {
                    IncidentId = incidentId,
                    TechnicalUserId = GetAuthenticatedUserId(),
                    Diagnosis = diagnosis,
                    RecommendedActions = recommendedActions
                },
                cancellationToken);

            TempData["IncidentSuccess"] =
                "El análisis técnico fue registrado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo registrar el análisis técnico de la incidencia {IncidentId}.",
                incidentId);

            TempData["IncidentError"] =
                exception.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id = incidentId });
    }

    private int GetAuthenticatedUserId()
    {
        var value = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(value!);
    }

    [Authorize(Roles = ManagementRoles)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int id,
        string estado,
        int? institucionAsignadaId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(estado))
        {
            TempData["IncidentError"] =
                "Debes seleccionar un estado.";

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        try
        {
            await _incidentsApiClient.UpdateStatusAsync(
                id,
                new UpdateIncidentStatusRequest
                {
                    Estado = estado,
                    InstitucionAsignadaId =
                        institucionAsignadaId
                },
                cancellationToken);

            TempData["IncidentSuccess"] =
                "El estado de la incidencia fue actualizado correctamente.";
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo actualizar la incidencia {IncidentId}.",
                id);

            TempData["IncidentError"] =
                exception.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}