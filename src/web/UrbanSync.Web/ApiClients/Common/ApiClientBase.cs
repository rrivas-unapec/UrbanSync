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
        using var response = await _httpClient.GetAsync(
            uri,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<T>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            uri,
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsJsonAsync(
            uri,
            request,
            JsonOptions,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TResponse>(
            JsonOptions,
            cancellationToken);
    }

    protected async Task PatchAsync(
        string uri,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync(
            uri,
            content: null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        var message = "La API de UrbanSync rechazó la operación.";

        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(
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
            errors.Values.SelectMany(messages => messages));
    }
}