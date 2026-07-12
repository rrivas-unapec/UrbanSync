using System.Net.Http.Json;
using System.Text.Json;

namespace UrbanSync.Web.Services;

public sealed class UrbanSyncApiClient : IUrbanSyncApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public UrbanSyncApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiLoginResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiLoginResponse>(_jsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<ApiUserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ApiUserDto>>("api/usuarios", _jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyList<ApiRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<List<ApiRoleDto>>("api/roles", _jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task<ApiUserDto?> CreateUserAsync(ApiCreateUserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/usuarios", request, _jsonOptions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<ApiUserDto>(_jsonOptions, cancellationToken);
    }

    public async Task<bool> ToggleUserStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync($"api/usuarios/{id}/toggle-status", null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var message = "La API de UrbanSync rechazo la operacion.";

        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(body, _jsonOptions);
                message = error?.Mensaje ?? error?.Message ?? error?.Title ?? error?.Detail ?? body;
            }
            catch
            {
                message = body;
            }
        }

        throw new InvalidOperationException(message);
    }
}
