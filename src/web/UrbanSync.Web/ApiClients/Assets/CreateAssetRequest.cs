namespace UrbanSync.Web.ApiClients.Assets;

public sealed class CreateAssetRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = "Operativo";

    public int JurisdictionId { get; set; }

    public DateTime? InstallationDate { get; set; }
}
