namespace UrbanSync.Web.ApiClients.WorkOrders;

public interface IWorkOrdersApiClient
{
    Task<IReadOnlyList<WorkOrderResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);
}