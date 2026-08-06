namespace UrbanSync.Web.ApiClients.Common;

public sealed class ApiErrorResponse
{
    public string? Mensaje { get; set; }

    public string? Message { get; set; }

    public string? Title { get; set; }

    public string? Detail { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }
}