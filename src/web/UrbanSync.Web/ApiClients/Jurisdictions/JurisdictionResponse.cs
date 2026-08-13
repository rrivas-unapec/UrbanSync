namespace UrbanSync.Web.ApiClients.Jurisdictions;

public sealed class JurisdictionResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public int? ParentJurisdictionId { get; set; }

    public string? ParentJurisdictionName { get; set; }

    public bool IsActive { get; set; }
}
