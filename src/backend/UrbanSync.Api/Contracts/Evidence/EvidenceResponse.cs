namespace UrbanSync.Api.Contracts.Evidence
{
    public sealed class EvidenceResponse
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public string EvidenceType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int UploadedByUserId { get; set; }

        public string UploadedByUserName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}
