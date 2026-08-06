using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Users;

public interface IUserManagementPageService
{
    Task<IReadOnlyList<UserListViewModel>> BuildListAsync(
        CancellationToken cancellationToken = default);

    Task<UserCreatePageViewModel> BuildCreatePageAsync(
        UserCreateViewModel? form = null,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        UserCreateViewModel form,
        CancellationToken cancellationToken = default);

    Task ToggleStatusAsync(
        int userId,
        CancellationToken cancellationToken = default);
}