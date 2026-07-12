namespace UrbanSync.Web.Services;

public interface IUrbanSyncApiClient
{
    Task<ApiLoginResponse?> LoginAsync(ApiLoginRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiRoleDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<ApiUserDto?> CreateUserAsync(ApiCreateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> ToggleUserStatusAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiReportSummaryDto?> GetReportsSummaryAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiIncidentDto>> GetIncidentsAsync(string? estado = null, CancellationToken cancellationToken = default);
    Task<ApiIncidentDto?> TriageIncidentAsync(int id, ApiIncidentTriageRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApiWorkOrderDto>> GetWorkOrdersAsync(CancellationToken cancellationToken = default);
}
