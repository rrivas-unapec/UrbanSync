namespace UrbanSync.Web.ApiClients.WorkOrders;

public interface IWorkOrdersApiClient
{
    Task<IReadOnlyList<WorkOrderResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<WorkOrderResponse?> CreateAsync(
        CreateWorkOrderRequest request,
        CancellationToken cancellationToken = default);

    Task<WorkOrderResponse?> UpdateStatusAsync(
        int id,
        UpdateWorkOrderStatusRequest request,
        CancellationToken cancellationToken = default);
}