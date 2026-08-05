using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace UrbanSync.Web.ApiClients.Common;

public abstract class ApiClientBase
{
    private readonly HttpClient _httpClient;

    protected ApiClientBase(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    protected static JsonSerializerOptions JsonOptions { get; } =
        new(JsonSerializerDefaults.Web);

    protected async Task<T?> GetAsync<T>(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            () => _httpClient.GetAsync(
                uri,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            () => _httpClient.PostAsJsonAsync(
                uri,
                request,
                JsonOptions,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            () => _httpClient.PatchAsJsonAsync(
                uri,
                request,
                JsonOptions,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task PatchAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(
            () => _httpClient.PatchAsync(
                uri,
                content: null,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessAsync(
            response,
            cancellationToken);
    }

    private static async Task<HttpResponseMessage> SendAsync(
        Func<Task<HttpResponseMessage>> sendRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            return await sendRequest();
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            throw new UrbanSyncApiException(
                "La API de UrbanSync tardó demasiado en responder.",
                HttpStatusCode.RequestTimeout,
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new UrbanSyncApiException(
                "No fue posible conectarse con la API de UrbanSync.",
                HttpStatusCode.ServiceUnavailable,
                exception);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        var message =
            "La API de UrbanSync rechazó la operación.";

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                var error =
                    JsonSerializer.Deserialize<ApiErrorResponse>(
                        responseBody,
                        JsonOptions);

                message =
                    GetValidationMessage(error?.Errors) ??
                    error?.Mensaje ??
                    error?.Message ??
                    error?.Detail ??
                    error?.Title ??
                    message;
            }
            catch (JsonException)
            {
                message = responseBody;
            }
        }

        throw new UrbanSyncApiException(
            message,
            response.StatusCode);
    }

    private static string? GetValidationMessage(
        Dictionary<string, string[]>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return null;
        }

        return string.Join(
            " ",
            errors.Values.SelectMany(
                messages => messages));
    }
}