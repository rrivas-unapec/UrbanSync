namespace UrbanSync.Web.ApiClients.Incidents;

public sealed class IncidentLocationRequest
{
    public decimal? Lat { get; set; }

    public decimal? Lng { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public string? Referencia { get; set; }

    public int? JurisdiccionId { get; set; }
}
