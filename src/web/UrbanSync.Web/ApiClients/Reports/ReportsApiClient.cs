using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Reports;

public sealed class ReportsApiClient
    : ApiClientBase,
      IReportsApiClient
{
    public ReportsApiClient(HttpClient httpClient)
        : base(httpClient)
    {
    }

    public Task<ReportSummaryResponse?> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        return GetAsync<ReportSummaryResponse>(
            "api/reports/summary",
            cancellationToken);
    }
}