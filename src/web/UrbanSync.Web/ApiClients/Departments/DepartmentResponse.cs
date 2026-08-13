namespace UrbanSync.Web.ApiClients.Departments;

public sealed class DepartmentResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int? JurisdictionId { get; set; }

    public string? JurisdictionName { get; set; }

    public bool IsActive { get; set; }
}
