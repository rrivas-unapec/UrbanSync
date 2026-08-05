using UrbanSync.Application.Features.Users;

namespace UrbanSync.Application.Features.Authentication;

public sealed class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public UsuarioDto User { get; set; } = new();
}