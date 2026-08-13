using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.WorkOrders;

public sealed class WorkOrdersApiClient
    : ApiClientBase,
      IWorkOrdersApiClient
{
    public WorkOrdersApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<WorkOrderResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var workOrders = await GetAsync<List<WorkOrderResponse>>(
            "api/jobs",
            cancellationToken);

        return workOrders ?? [];
    }

    public Task<WorkOrderResponse?> CreateAsync(
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        return PostAsync<CreateWorkOrderRequest, WorkOrderResponse>(
            "api/jobs",
            request,
            cancellationToken);
    }

    public Task<WorkOrderResponse?> UpdateStatusAsync(
        int id,
        UpdateWorkOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        return PutAsync<UpdateWorkOrderStatusRequest, WorkOrderResponse>(
            $"api/jobs/{id}",
            request,
            cancellationToken);
    }
}