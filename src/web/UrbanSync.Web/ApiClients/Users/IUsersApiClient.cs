namespace UrbanSync.Web.ApiClients.Users;

public interface IUsersApiClient
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<UserResponse?> CreateAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken = default);

    Task ToggleStatusAsync(
        int id,
        CancellationToken cancellationToken = default);
}