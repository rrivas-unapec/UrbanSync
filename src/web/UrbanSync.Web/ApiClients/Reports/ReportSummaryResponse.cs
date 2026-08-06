namespace UrbanSync.Web.ApiClients.Reports;

public sealed class ReportSummaryResponse
{
    public int Total { get; set; }

    public List<ReportCountResponse> PorEstado { get; set; } = [];

    public List<ReportCountResponse> PorTipo { get; set; } = [];

    public List<ReportCountResponse> PorPrioridad { get; set; } = [];

    public List<ReportCountResponse> PorJurisdiccion { get; set; } = [];
}