namespace UrbanSync.Web.ApiClients.Activity;

public interface IActivityApiClient
{
    Task<IReadOnlyList<ActivityResponse>> GetAllAsync(
        int? usuarioId = null,
        string? entidad = null,
        string? accion = null,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        CancellationToken cancellationToken = default);

    Task<ActivityResponse?> GetByIdAsync(
        long id,
        CancellationToken cancellationToken = default);
}