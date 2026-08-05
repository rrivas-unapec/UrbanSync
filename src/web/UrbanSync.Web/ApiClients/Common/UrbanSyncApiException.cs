using System.Net;

namespace UrbanSync.Web.ApiClients.Common;

public sealed class UrbanSyncApiException : Exception
{
    public UrbanSyncApiException(
        string message,
        HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public UrbanSyncApiException(
        string message,
        HttpStatusCode statusCode,
        Exception innerException)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}