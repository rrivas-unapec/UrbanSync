namespace UrbanSync.Web.ViewModels;

public sealed class UserCreatePageViewModel
{
    public UserCreateViewModel Form { get; set; } = new();

    public IReadOnlyList<RoleOptionViewModel> Roles { get; set; } =
        [];
}

public sealed class RoleOptionViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}