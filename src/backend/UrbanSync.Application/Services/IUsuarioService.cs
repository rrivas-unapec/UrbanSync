using UrbanSync.Domain.DTOs;

namespace UrbanSync.Application.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> GetAllAsync();
        Task<UsuarioDto?> GetByIdAsync(int id);
        Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<bool> ToggleStatusAsync(int id);
    }
}
