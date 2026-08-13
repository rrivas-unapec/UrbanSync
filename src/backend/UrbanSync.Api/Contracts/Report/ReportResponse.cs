namespace UrbanSync.Api.Contracts.Report
{
    public sealed class ReportResponse
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public int? JobId { get; set; }

        public int GeneratedByUserId { get; set; }

        public string GeneratedByUserName { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? FilePath { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
