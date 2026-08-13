namespace UrbanSync.Web.ApiClients.IncidentTypes;

public sealed class IncidentTypeResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int InstitutionId { get; set; }

    public string InstitutionName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
