using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.ApiClients.Common;
using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.Reports;
using UrbanSync.Web.ApiClients.WorkOrders;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private static readonly string[] EstadosActivos =
    [
        "Registrada",
        "EnAnalisis",
        "Asignada",
        "EnProceso"
    ];

    private static readonly string[] PaletaTipos =
    [
        "#0057B8",
        "#00A676",
        "#FFB800",
        "#E4572E",
        "#7C3AED"
    ];

    private static readonly Dictionary<string, string> AccionesModeracion =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["aprobar"] = "asignar",
            ["rechazar"] = "rechazar"
        };

    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<DashboardController> _logger;
    private readonly IReportsApiClient _reportsApiClient;
    private readonly IIncidentsApiClient _incidentsApiClient;
    private readonly IWorkOrdersApiClient _workOrdersApiClient;

    public DashboardController(
        IReportsApiClient reportsApiClient,
        IIncidentsApiClient incidentsApiClient,
        IWorkOrdersApiClient workOrdersApiClient,
        ActivityLogger activityLogger,
        ILogger<DashboardController> logger)
    {
        _reportsApiClient = reportsApiClient;
        _incidentsApiClient = incidentsApiClient;
        _workOrdersApiClient = workOrdersApiClient;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (User.IsInRole("Administrador"))
        {
            var model = await BuildPanelPrincipalAsync(
                cancellationToken);

            return View("Administrador", model);
        }

        if (User.IsInRole("Supervisor") ||
            User.IsInRole("SupervisorOperaciones"))
        {
            return View("Supervisor");
        }

        if (User.IsInRole("Tecnico") ||
            User.IsInRole("AnalistaTecnico"))
        {
            return View("Tecnico");
        }

        return View("Ciudadano");
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Mapa(
        CancellationToken cancellationToken)
    {
        var model = await BuildMapaAsync(cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivosOrdenes(
        CancellationToken cancellationToken)
    {
        var model = await BuildActivosOrdenesAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Rutas(
        CancellationToken cancellationToken)
    {
        var model = await BuildRutasAsync(cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Moderacion(
        CancellationToken cancellationToken)
    {
        var model = await BuildModeracionAsync(
            cancellationToken);

        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModeracionAccion(
        int id,
        string accion,
        CancellationToken cancellationToken)
    {
        if (!AccionesModeracion.TryGetValue(
                accion,
                out var accionApi))
        {
            return BadRequest();
        }

        try
        {
            await _incidentsApiClient.TriageAsync(
                id,
                new TriageIncidentRequest
                {
                    Accion = accionApi
                },
                cancellationToken);

            await _activityLogger.LogAsync(
                "Moderación de reporte",
                $"Se aplicó la acción '{accion}' al reporte #{id}.");
        }
        catch (UrbanSyncApiException exception)
        {
            _logger.LogWarning(
                exception,
                "La API rechazó la acción {Accion} para el reporte {Id}.",
                accion,
                id);

            TempData["ModeracionError"] = exception.Message;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo aplicar la acción {Accion} al reporte {Id}.",
                accion,
                id);

            TempData["ModeracionError"] =
                "No se pudo comunicar con la API de UrbanSync. Intenta de nuevo.";
        }

        return RedirectToAction(nameof(Moderacion));
    }

    private async Task<PanelPrincipalViewModel>
        BuildPanelPrincipalAsync(
            CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _reportsApiClient.GetSummaryAsync(
                cancellationToken);

            return new PanelPrincipalViewModel
            {
                DatosDisponibles = true,
                TotalReportes = summary?.Total ?? 0,
                OrdenesActivas = summary?.PorEstado
                    .Where(estado =>
                        EstadosActivos.Contains(
                            estado.Clave,
                            StringComparer.OrdinalIgnoreCase))
                    .Sum(estado => estado.Total) ?? 0
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo obtener el resumen de reportes desde la API de UrbanSync.");

            return new PanelPrincipalViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    private async Task<MapaViewModel> BuildMapaAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var incidentes = await _incidentsApiClient.GetAllAsync(
                cancellationToken: cancellationToken);

            var tipos = incidentes
                .Select(incidente => incidente.TipoIncidencia)
                .Where(tipo => !string.IsNullOrWhiteSpace(tipo))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tipo => tipo)
                .ToList();

            var colorPorTipo = tipos
                .Select((tipo, index) => new
                {
                    Tipo = tipo,
                    Color = PaletaTipos[index % PaletaTipos.Length]
                })
                .ToDictionary(
                    item => item.Tipo,
                    item => item.Color,
                    StringComparer.OrdinalIgnoreCase);

            var puntos = incidentes
                .Where(incidente =>
                    incidente.Latitud.HasValue &&
                    incidente.Longitud.HasValue)
                .Select(incidente =>
                    new IncidentMapPointViewModel
                    {
                        Id = incidente.Id,
                        Lat = incidente.Latitud!.Value,
                        Lng = incidente.Longitud!.Value,
                        CodigoCaso = incidente.CodigoCaso,
                        TipoIncidencia = incidente.TipoIncidencia,
                        Prioridad = incidente.Prioridad,
                        Direccion = incidente.Direccion,
                        Color = colorPorTipo.GetValueOrDefault(
                            incidente.TipoIncidencia,
                            PaletaTipos[0])
                    })
                .ToList();

            return new MapaViewModel
            {
                DatosDisponibles = true,
                Tipos = tipos
                    .Select(tipo => new MapaTipoViewModel
                    {
                        Nombre = tipo,
                        Color = colorPorTipo[tipo]
                    })
                    .ToList(),
                Puntos = puntos
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron obtener las incidencias para el mapa desde la API de UrbanSync.");

            return new MapaViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    private async Task<ActivosOrdenesViewModel>
        BuildActivosOrdenesAsync(
            CancellationToken cancellationToken)
    {
        try
        {
            var ordenes = await _workOrdersApiClient.GetAllAsync(
                cancellationToken);

            return new ActivosOrdenesViewModel
            {
                DatosDisponibles = true,
                Ordenes = ordenes
                    .Select(orden => new WorkOrderItemViewModel
                    {
                        Id = orden.Id,
                        CodigoCaso = orden.CodigoCaso,
                        DescripcionTrabajo =
                            orden.DescripcionTrabajo,
                        UsuarioAsignado =
                            orden.UsuarioAsignado,
                        Estado = orden.Estado,
                        FechaInicio = orden.FechaInicio,
                        FechaFin = orden.FechaFin,
                        Resultado = orden.Resultado
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron obtener las órdenes de trabajo desde la API de UrbanSync.");

            return new ActivosOrdenesViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    private async Task<RutasViewModel> BuildRutasAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var ordenes = await _workOrdersApiClient.GetAllAsync(
                cancellationToken);

            var cuadrillas = ordenes
                .Where(orden =>
                    !string.IsNullOrWhiteSpace(
                        orden.UsuarioAsignado))
                .GroupBy(
                    orden => orden.UsuarioAsignado,
                    StringComparer.OrdinalIgnoreCase)
                .Select(grupo =>
                {
                    var pendientes = grupo.Count(orden =>
                        string.Equals(
                            orden.Estado,
                            "Pendiente",
                            StringComparison.OrdinalIgnoreCase));

                    var enProgreso = grupo.Count(orden =>
                        string.Equals(
                            orden.Estado,
                            "EnProgreso",
                            StringComparison.OrdinalIgnoreCase));

                    var finalizadas = grupo.Count(orden =>
                        string.Equals(
                            orden.Estado,
                            "Finalizado",
                            StringComparison.OrdinalIgnoreCase));

                    return new CuadrillaViewModel
                    {
                        Tecnico = grupo.Key,
                        TotalOrdenes = grupo.Count(),
                        Pendientes = pendientes,
                        EnProgreso = enProgreso,
                        Finalizadas = finalizadas,
                        Estado = enProgreso > 0
                            ? "Trabajo en curso"
                            : pendientes > 0
                                ? "Pendiente de iniciar"
                                : "Sin trabajo activo"
                    };
                })
                .OrderByDescending(cuadrilla =>
                    cuadrilla.EnProgreso)
                .ThenByDescending(cuadrilla =>
                    cuadrilla.Pendientes)
                .ToList();

            return new RutasViewModel
            {
                DatosDisponibles = true,
                Cuadrillas = cuadrillas
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudieron obtener las órdenes para agrupar las cuadrillas.");

            return new RutasViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    private async Task<ModeracionViewModel>
        BuildModeracionAsync(
            CancellationToken cancellationToken)
    {
        try
        {
            var incidentes = await _incidentsApiClient.GetAllAsync(
                "Registrada",
                cancellationToken);

            return new ModeracionViewModel
            {
                DatosDisponibles = true,
                Cola = incidentes
                    .Select(incidente =>
                        new IncidentQueueItemViewModel
                        {
                            Id = incidente.Id,
                            CodigoCaso = incidente.CodigoCaso,
                            TipoIncidencia =
                                incidente.TipoIncidencia,
                            Prioridad = incidente.Prioridad,
                            Direccion = incidente.Direccion,
                            Jurisdiccion =
                                incidente.Jurisdiccion,
                            UsuarioReporta =
                                incidente.UsuarioReporta,
                            FechaReporte =
                                incidente.FechaReporte
                        })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo obtener la cola de moderación desde la API de UrbanSync.");

            return new ModeracionViewModel
            {
                DatosDisponibles = false
            };
        }
    }
}