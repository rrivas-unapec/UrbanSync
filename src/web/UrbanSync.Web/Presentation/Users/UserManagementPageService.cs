using UrbanSync.Web.ApiClients.Roles;
using UrbanSync.Web.ApiClients.Users;
using UrbanSync.Web.ViewModels;

namespace UrbanSync.Web.Presentation.Users;

public sealed class UserManagementPageService
    : IUserManagementPageService
{
    private readonly IUsersApiClient _usersApiClient;
    private readonly IRolesApiClient _rolesApiClient;

    public UserManagementPageService(
        IUsersApiClient usersApiClient,
        IRolesApiClient rolesApiClient)
    {
        _usersApiClient = usersApiClient;
        _rolesApiClient = rolesApiClient;
    }

    public async Task<IReadOnlyList<UserListViewModel>> BuildListAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _usersApiClient.GetAllAsync(
            cancellationToken);

        return users
            .Select(user => new UserListViewModel
            {
                Id = user.Id,
                FullName = user.NombreCompleto,
                Email = user.Email,
                Role = user.RolNombre,
                IsActive = user.Activo
            })
            .ToList();
    }

    public async Task<UserCreatePageViewModel> BuildCreatePageAsync(
        UserCreateViewModel? form = null,
        CancellationToken cancellationToken = default)
    {
        var roles = await _rolesApiClient.GetAllAsync(
            cancellationToken);

        return new UserCreatePageViewModel
        {
            Form = form ?? new UserCreateViewModel(),
            Roles = roles
                .OrderBy(role => role.Nombre)
                .Select(role => new RoleOptionViewModel
                {
                    Id = role.Id,
                    Name = role.Nombre
                })
                .ToList()
        };
    }

    public async Task CreateAsync(
        UserCreateViewModel form,
        CancellationToken cancellationToken = default)
    {
        await _usersApiClient.CreateAsync(
            new CreateUserRequest
            {
                NombreUsuario = form.Email.Trim(),
                NombreCompleto = form.FullName.Trim(),
                Email = form.Email.Trim(),
                Password = form.Password,
                RolId = form.RoleId
            },
            cancellationToken);
    }

    public Task ToggleStatusAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return _usersApiClient.ToggleStatusAsync(
            userId,
            cancellationToken);
    }
}