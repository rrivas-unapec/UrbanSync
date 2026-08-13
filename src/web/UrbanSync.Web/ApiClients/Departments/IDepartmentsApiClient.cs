namespace UrbanSync.Web.ApiClients.Departments;

public interface IDepartmentsApiClient
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<DepartmentResponse?> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default);
}
