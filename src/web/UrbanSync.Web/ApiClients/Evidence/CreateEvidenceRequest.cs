namespace UrbanSync.Web.ApiClients.Evidence;

public sealed class CreateEvidenceRequest
{
    public int IncidentId { get; set; }

    public string EvidenceType { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int UploadedByUserId { get; set; }
}
