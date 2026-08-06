namespace UrbanSync.Application.Features.Incidents;

public sealed class UpdateIncidentStatusDto
{
    public string Estado { get; set; } = string.Empty;

    public int? InstitucionAsignadaId { get; set; }
}