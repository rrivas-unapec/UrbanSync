using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanSync.Web.Services;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly string[] EstadosActivos = ["Registrada", "EnAnalisis", "Asignada", "EnProceso"];
    private static readonly string[] PaletaTipos = ["#0057B8", "#00A676", "#FFB800", "#E4572E", "#7C3AED"];
    private static readonly Dictionary<string, string> AccionesModeracion = new()
    {
        ["aprobar"] = "asignar",
        ["rechazar"] = "rechazar"
    };

    private readonly IUrbanSyncApiClient _api;
    private readonly ActivityLogger _activityLogger;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IUrbanSyncApiClient api, ActivityLogger activityLogger, ILogger<DashboardController> logger)
    {
        _api = api;
        _activityLogger = activityLogger;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Administrador"))
        {
            var model = await BuildPanelPrincipalAsync();
            return View("Administrador", model);
        }

        if (User.IsInRole("Supervisor") || User.IsInRole("SupervisorOperaciones"))
            return View("Supervisor");

        if (User.IsInRole("Tecnico") || User.IsInRole("AnalistaTecnico"))
            return View("Tecnico");

        return View("Ciudadano");
    }

    private async Task<PanelPrincipalViewModel> BuildPanelPrincipalAsync()
    {
        try
        {
            var summary = await _api.GetReportsSummaryAsync();

            return new PanelPrincipalViewModel
            {
                DatosDisponibles = true,
                TotalReportes = summary?.Total ?? 0,
                OrdenesActivas = summary?.PorEstado
                    .Where(e => EstadosActivos.Contains(e.Clave))
                    .Sum(e => e.Total) ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener el resumen de reportes desde la API de UrbanSync.");
            return new PanelPrincipalViewModel { DatosDisponibles = false };
        }
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Mapa()
    {
        var model = await BuildMapaAsync();
        return View(model);
    }

    private async Task<MapaViewModel> BuildMapaAsync()
    {
        try
        {
            var incidentes = await _api.GetIncidentsAsync();

            var tipos = incidentes
                .Select(i => i.TipoIncidencia)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            var colorPorTipo = tipos
                .Select((tipo, index) => (tipo, color: PaletaTipos[index % PaletaTipos.Length]))
                .ToDictionary(x => x.tipo, x => x.color);

            var puntos = incidentes
                .Where(i => i.Latitud.HasValue && i.Longitud.HasValue)
                .Select(i => new IncidentMapPointViewModel
                {
                    Id = i.Id,
                    Lat = i.Latitud!.Value,
                    Lng = i.Longitud!.Value,
                    CodigoCaso = i.CodigoCaso,
                    TipoIncidencia = i.TipoIncidencia,
                    Prioridad = i.Prioridad,
                    Direccion = i.Direccion,
                    Color = colorPorTipo.GetValueOrDefault(i.TipoIncidencia, PaletaTipos[0])
                })
                .ToList();

            return new MapaViewModel
            {
                DatosDisponibles = true,
                Tipos = tipos.Select(t => new MapaTipoViewModel { Nombre = t, Color = colorPorTipo[t] }).ToList(),
                Puntos = puntos
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener las incidencias para el mapa desde la API de UrbanSync.");
            return new MapaViewModel { DatosDisponibles = false };
        }
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActivosOrdenes()
    {
        var model = await BuildActivosOrdenesAsync();
        return View(model);
    }

    private async Task<ActivosOrdenesViewModel> BuildActivosOrdenesAsync()
    {
        try
        {
            var ordenes = await _api.GetWorkOrdersAsync();

            return new ActivosOrdenesViewModel
            {
                DatosDisponibles = true,
                Ordenes = ordenes.Select(o => new WorkOrderItemViewModel
                {
                    Id = o.Id,
                    CodigoCaso = o.CodigoCaso,
                    DescripcionTrabajo = o.DescripcionTrabajo,
                    UsuarioAsignado = o.UsuarioAsignado,
                    Estado = o.Estado,
                    FechaInicio = o.FechaInicio,
                    FechaFin = o.FechaFin,
                    Resultado = o.Resultado
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener las ordenes de trabajo desde la API de UrbanSync.");
            return new ActivosOrdenesViewModel { DatosDisponibles = false };
        }
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Rutas()
    {
        var model = await BuildRutasAsync();
        return View(model);
    }

    private async Task<RutasViewModel> BuildRutasAsync()
    {
        try
        {
            var ordenes = await _api.GetWorkOrdersAsync();

            var cuadrillas = ordenes
                .Where(o => !string.IsNullOrWhiteSpace(o.UsuarioAsignado))
                .GroupBy(o => o.UsuarioAsignado)
                .Select(g =>
                {
                    var pendientes = g.Count(o => o.Estado == "Pendiente");
                    var enProgreso = g.Count(o => o.Estado == "EnProgreso");
                    var finalizadas = g.Count(o => o.Estado == "Finalizado");

                    return new CuadrillaViewModel
                    {
                        Tecnico = g.Key,
                        TotalOrdenes = g.Count(),
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
                .OrderByDescending(c => c.EnProgreso)
                .ThenByDescending(c => c.Pendientes)
                .ToList();

            return new RutasViewModel { DatosDisponibles = true, Cuadrillas = cuadrillas };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener las ordenes de trabajo para agrupar cuadrillas desde la API de UrbanSync.");
            return new RutasViewModel { DatosDisponibles = false };
        }
    }

    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Moderacion()
    {
        var model = await BuildModeracionAsync();
        return View(model);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModeracionAccion(int id, string accion)
    {
        if (!AccionesModeracion.TryGetValue(accion, out var accionApi))
            return BadRequest();

        try
        {
            await _api.TriageIncidentAsync(id, new ApiIncidentTriageRequest { Accion = accionApi });
            await _activityLogger.LogAsync("Moderacion de reporte", $"Se aplico la accion '{accion}' al reporte #{id}.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo aplicar la accion {Accion} al reporte {Id}.", accion, id);
            TempData["ModeracionError"] = "No se pudo comunicar con la API de UrbanSync. Intenta de nuevo.";
        }

        return RedirectToAction(nameof(Moderacion));
    }

    private async Task<ModeracionViewModel> BuildModeracionAsync()
    {
        try
        {
            var incidentes = await _api.GetIncidentsAsync("Registrada");

            return new ModeracionViewModel
            {
                DatosDisponibles = true,
                Cola = incidentes.Select(i => new IncidentQueueItemViewModel
                {
                    Id = i.Id,
                    CodigoCaso = i.CodigoCaso,
                    TipoIncidencia = i.TipoIncidencia,
                    Prioridad = i.Prioridad,
                    Direccion = i.Direccion,
                    Jurisdiccion = i.Jurisdiccion,
                    UsuarioReporta = i.UsuarioReporta,
                    FechaReporte = i.FechaReporte
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener la cola de moderacion desde la API de UrbanSync.");
            return new ModeracionViewModel { DatosDisponibles = false };
        }
    }
}
