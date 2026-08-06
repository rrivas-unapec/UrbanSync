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
            "api/work-orders",
            cancellationToken);

        return workOrders ?? [];
    }
}