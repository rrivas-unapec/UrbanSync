using UrbanSync.Application.Features.Authentication;

namespace UrbanSync.Application.Features.Users;

public interface IUsuarioService
{
    Task<IEnumerable<UsuarioDto>> GetAllAsync();

    Task<UsuarioDto?> GetByIdAsync(int id);

    Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);

    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);

    Task<bool> ToggleStatusAsync(int id);

    Task ChangePasswordAsync(ChangePasswordDto dto);
}