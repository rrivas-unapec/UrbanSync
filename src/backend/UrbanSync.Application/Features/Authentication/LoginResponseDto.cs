using UrbanSync.Application.Features.Users;

namespace UrbanSync.Application.Features.Authentication;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public UsuarioDto User { get; set; } = new();
}