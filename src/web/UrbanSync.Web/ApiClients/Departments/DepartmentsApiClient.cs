using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Departments;

public sealed class DepartmentsApiClient
    : ApiClientBase,
      IDepartmentsApiClient
{
    public DepartmentsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var departments = await GetAsync<List<DepartmentResponse>>(
            "api/departments",
            cancellationToken);

        return departments ?? [];
    }

    public Task<DepartmentResponse?> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateDepartmentRequest, DepartmentResponse>(
            "api/departments",
            request,
            cancellationToken);
    }
}
