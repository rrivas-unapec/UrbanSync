namespace UrbanSync.Application.Features.Audit;

public sealed class AuditDto
{
    public long Id { get; set; }

    public int? UserId { get; set; }

    public string? UserName { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? Entity { get; set; }

    public int? EntityId { get; set; }

    public string? Detail { get; set; }

    public string? IpAddress { get; set; }

    public DateTime Timestamp { get; set; }
}