namespace UrbanSync.Web.ApiClients.Reports;

public interface IReportsApiClient
{
    Task<ReportSummaryResponse?> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}