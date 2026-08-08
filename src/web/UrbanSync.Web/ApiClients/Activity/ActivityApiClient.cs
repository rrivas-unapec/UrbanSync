using System.Globalization;
using UrbanSync.Web.ApiClients.Common;

namespace UrbanSync.Web.ApiClients.Activity;

public sealed class ActivityApiClient
    : ApiClientBase,
      IActivityApiClient
{
    public ActivityApiClient(
        HttpClient httpClient)
        : base(httpClient)
    {
    }

    public async Task<IReadOnlyList<ActivityResponse>> GetAllAsync(
        int? usuarioId = null,
        string? entidad = null,
        string? accion = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();

        if (usuarioId.HasValue)
        {
            query.Add(
                $"usuarioId={usuarioId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(entidad))
        {
            query.Add(
                $"entidad={Uri.EscapeDataString(entidad)}");
        }

        if (!string.IsNullOrWhiteSpace(accion))
        {
            query.Add(
                $"accion={Uri.EscapeDataString(accion)}");
        }

        if (fechaInicio.HasValue)
        {
            var value = fechaInicio.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);

            query.Add(
                $"fechaInicio={Uri.EscapeDataString(value)}");
        }

        if (fechaFin.HasValue)
        {
            var value = fechaFin.Value.ToString(
                "O",
                CultureInfo.InvariantCulture);

            query.Add(
                $"fechaFin={Uri.EscapeDataString(value)}");
        }

        var uri = "api/activity";

        if (query.Count > 0)
        {
            uri += $"?{string.Join("&", query)}";
        }

        var activities =
            await GetAsync<List<ActivityResponse>>(
                uri,
                cancellationToken);

        return activities ?? [];
    }

    public Task<ActivityResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        return GetAsync<ActivityResponse>(
            $"api/activity/{id}",
            cancellationToken);
    }
}