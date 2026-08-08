namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class UpdateIncidentStatusRequest
{
    public string Estado { get; set; } =
        string.Empty;

    public int? InstitucionAsignadaId { get; set; }
}