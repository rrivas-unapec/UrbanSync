namespace UrbanSync.Web.ApiClients.Departments;

public sealed class CreateDepartmentRequest
{
    public string Name { get; set; } = string.Empty;

    public int? JurisdictionId { get; set; }
}
