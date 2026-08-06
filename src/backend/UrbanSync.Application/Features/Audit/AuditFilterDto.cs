namespace UrbanSync.Application.Features.Audit;

public sealed class AuditFilterDto
{
    public int? UserId { get; set; }

    public string? Entity { get; set; }

    public string? Action { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}