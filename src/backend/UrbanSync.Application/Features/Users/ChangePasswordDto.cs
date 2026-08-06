namespace UrbanSync.Application.Features.Users;

public sealed class ChangePasswordDto
{
    public int UserId { get; set; }

    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}