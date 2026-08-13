namespace UrbanSync.Web.ApiClients.WorkOrders;

public sealed class UpdateWorkOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Result { get; set; }
}
