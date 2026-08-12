namespace UrbanSync.Api.Contracts.Report
{
    public sealed class CreateReportRequest
    {
        public int IncidentId { get; set; }

        public int? JobId { get; set; }

        public int GeneratedByUserId { get; set; }

        public string? Content { get; set; }

        public string? FilePath { get; set; }
    }
}
