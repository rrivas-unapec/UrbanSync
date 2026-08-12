using System.Text.Json.Serialization;

namespace UrbanSync.Web.ApiClients.WorkOrders;

public sealed class WorkOrderResponse
{
    public int Id { get; set; }

    public string CodigoCaso { get; set; } = string.Empty;

    [JsonPropertyName("assignedUserName")]
    public string UsuarioAsignado { get; set; } = string.Empty;

    [JsonPropertyName("jobDescription")]
    public string DescripcionTrabajo { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Estado { get; set; } = string.Empty;

    [JsonPropertyName("startDate")]
    public DateTime? FechaInicio { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? FechaFin { get; set; }

    [JsonPropertyName("result")]
    public string? Resultado { get; set; }
}
