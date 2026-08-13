using UrbanSync.Web.ApiClients.Departments;
using UrbanSync.Web.ApiClients.Jurisdictions;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Departments;

public sealed class DepartmentsPageService : IDepartmentsPageService
{
    private readonly IDepartmentsApiClient _departmentsApiClient;
    private readonly IJurisdictionsApiClient _jurisdictionsApiClient;
    private readonly ILogger<DepartmentsPageService> _logger;

    public DepartmentsPageService(
        IDepartmentsApiClient departmentsApiClient,
        IJurisdictionsApiClient jurisdictionsApiClient,
        ILogger<DepartmentsPageService> logger)
    {
        _departmentsApiClient = departmentsApiClient;
        _jurisdictionsApiClient = jurisdictionsApiClient;
        _logger = logger;
    }

    public async Task<DepartmentsViewModel> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var departmentsTask = _departmentsApiClient.GetAllAsync(
                cancellationToken);

            var jurisdictionsTask = _jurisdictionsApiClient.GetAllAsync(
                cancellationToken);

            await Task.WhenAll(departmentsTask, jurisdictionsTask);

            var departments = departmentsTask.Result;
            var jurisdictions = jurisdictionsTask.Result;

            return new DepartmentsViewModel
            {
                DatosDisponibles = true,
                Departamentos = departments
                    .Select(department => new DepartmentItemViewModel
                    {
                        Id = department.Id,
                        Name = department.Name,
                        JurisdictionName = department.JurisdictionName,
                        IsActive = department.IsActive
                    })
                    .ToList(),
                OpcionesJurisdiccion = jurisdictions
                    .OrderBy(jurisdiction => jurisdiction.Name)
                    .Select(jurisdiction => new JurisdictionOptionViewModel
                    {
                        Id = jurisdiction.Id,
                        Name = jurisdiction.Name
                    })
                    .ToList()
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo construir el listado de departamentos.");

            return new DepartmentsViewModel
            {
                DatosDisponibles = false
            };
        }
    }

    public Task CreateAsync(
        string name,
        int? jurisdictionId,
        CancellationToken cancellationToken = default)
    {
        return _departmentsApiClient.CreateAsync(
            new CreateDepartmentRequest
            {
                Name = name,
                JurisdictionId = jurisdictionId
            },
            cancellationToken);
    }
}
