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

    public HttpStatusCode StatusCode { get; }
}