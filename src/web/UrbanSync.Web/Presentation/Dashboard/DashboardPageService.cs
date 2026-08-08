using UrbanSync.Web.ApiClients.Incidents;
using UrbanSync.Web.ApiClients.Reports;
using UrbanSync.Web.ApiClients.WorkOrders;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Dashboard;

public sealed class DashboardPageService : IDashboardPageService
{
    private static readonly string[] ActiveStatuses =
    [
        "Registrada",
        "EnAnalisis",
        "Asignada",
        "EnProceso"
    ];

    private static readonly string[] IncidentTypeColors =
    [
        "#0057B8",
        "#00A676",
        "#FFB800",
        "#E4572E",
        "#7C3AED"
    ];

    private readonly IReportsApiClient _reportsApiClient;
    private readonly IIncidentsApiClient _incidentsApiClient;
    private readonly IWorkOrdersApiClient _workOrdersApiClient;
    private readonly ILogger<DashboardPageService> _logger;

    public DashboardPageService(
        IReportsApiClient reportsApiClient,
        IIncidentsApiClient incidentsApiClient,
        IWorkOrdersApiClient workOrdersApiClient,
        ILogger<DashboardPageService> logger)
    {
        _reportsApiClient = reportsApiClient;
        _incidentsApiClient = incidentsApiClient;
        _workOrdersApiClient = workOrdersApiClient;
        _logger = logger;
    }

    public async Task<PanelPrincipalViewModel> BuildMainPanelAsync(
        CancellationToken cancellationToken = default)
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
                    .Where(status =>
                        ActiveStatuses.Contains(
                            status.Clave,
                            StringComparer.OrdinalIgnoreCase))
                    .Sum(status => status.Total) ?? 0
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir el panel principal.");

            return new PanelPrincipalViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public async Task<MapaViewModel> BuildMapAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var incidents = await _incidentsApiClient.GetAllAsync(
                cancellationToken: cancellationToken);

            var types = incidents
                .Select(incident => incident.TipoIncidencia)
                .Where(type => !string.IsNullOrWhiteSpace(type))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(type => type)
                .ToList();

            var colorByType = types
                .Select((type, index) => new
                {
                    Type = type,
                    Color = IncidentTypeColors[
                        index % IncidentTypeColors.Length]
                })
                .ToDictionary(
                    item => item.Type,
                    item => item.Color,
                    StringComparer.OrdinalIgnoreCase);

            var points = incidents
                .Where(incident =>
                    incident.Latitud.HasValue &&
                    incident.Longitud.HasValue)
                .Select(incident => new IncidentMapPointViewModel
                {
                    Id = incident.Id,
                    Lat = incident.Latitud!.Value,
                    Lng = incident.Longitud!.Value,
                    CodigoCaso = incident.CodigoCaso,
                    TipoIncidencia = incident.TipoIncidencia,
                    Prioridad = incident.Prioridad,
                    Direccion = incident.Direccion,
                    Color = colorByType.GetValueOrDefault(
                        incident.TipoIncidencia,
                        IncidentTypeColors[0])
                })
                .ToList();

            return new MapaViewModel
            {
                DatosDisponibles = true,
                Tipos = types
                    .Select(type => new MapaTipoViewModel
                    {
                        Nombre = type,
                        Color = colorByType[type]
                    })
                    .ToList(),
                Puntos = points
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir la pantalla del mapa.");

            return new MapaViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public async Task<ActivosOrdenesViewModel>
        BuildActiveWorkOrdersAsync(
            CancellationToken cancellationToken = default)
    {
        try
        {
            var workOrders = await _workOrdersApiClient.GetAllAsync(
                cancellationToken);

            return new ActivosOrdenesViewModel
            {
                DatosDisponibles = true,
                Ordenes = workOrders
                    .Select(workOrder => new WorkOrderItemViewModel
                    {
                        Id = workOrder.Id,
                        CodigoCaso = workOrder.CodigoCaso,
                        DescripcionTrabajo =
                            workOrder.DescripcionTrabajo,
                        UsuarioAsignado =
                            workOrder.UsuarioAsignado,
                        Estado = workOrder.Estado,
                        FechaInicio = workOrder.FechaInicio,
                        FechaFin = workOrder.FechaFin,
                        Resultado = workOrder.Resultado
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir la pantalla de órdenes activas.");

            return new ActivosOrdenesViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public async Task<RutasViewModel> BuildRoutesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var workOrders = await _workOrdersApiClient.GetAllAsync(
                cancellationToken);

            var crews = workOrders
                .Where(workOrder =>
                    !string.IsNullOrWhiteSpace(
                        workOrder.UsuarioAsignado))
                .GroupBy(
                    workOrder => workOrder.UsuarioAsignado,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var pending = group.Count(workOrder =>
                        HasStatus(workOrder.Estado, "Pendiente"));

                    var inProgress = group.Count(workOrder =>
                        HasStatus(workOrder.Estado, "EnProgreso"));

                    var completed = group.Count(workOrder =>
                        HasStatus(workOrder.Estado, "Finalizado"));

                    return new CuadrillaViewModel
                    {
                        Tecnico = group.Key,
                        TotalOrdenes = group.Count(),
                        Pendientes = pending,
                        EnProgreso = inProgress,
                        Finalizadas = completed,
                        Estado = inProgress > 0
                            ? "Trabajo en curso"
                            : pending > 0
                                ? "Pendiente de iniciar"
                                : "Sin trabajo activo"
                    };
                })
                .OrderByDescending(crew => crew.EnProgreso)
                .ThenByDescending(crew => crew.Pendientes)
                .ToList();

            return new RutasViewModel
            {
                DatosDisponibles = true,
                Cuadrillas = crews
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir la pantalla de rutas.");

            return new RutasViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public async Task<ModeracionViewModel> BuildModerationQueueAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var incidents = await _incidentsApiClient.GetAllAsync(
                status: "Registrada",
                cancellationToken: cancellationToken);

            return new ModeracionViewModel
            {
                DatosDisponibles = true,
                Cola = incidents
                    .Select(incident =>
                        new IncidentQueueItemViewModel
                        {
                            Id = incident.Id,
                            CodigoCaso = incident.CodigoCaso,
                            TipoIncidencia =
                                incident.TipoIncidencia,
                            Prioridad = incident.Prioridad,
                            Direccion = incident.Direccion,
                            Jurisdiccion =
                                incident.Jurisdiccion,
                            UsuarioReporta =
                                incident.UsuarioReporta,
                            FechaReporte =
                                incident.FechaReporte
                        })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir la cola de moderación.");

            return new ModeracionViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    private static bool HasStatus(
        string currentStatus,
        string expectedStatus)
    {
        return string.Equals(
            currentStatus,
            expectedStatus,
            StringComparison.OrdinalIgnoreCase);
    }
}