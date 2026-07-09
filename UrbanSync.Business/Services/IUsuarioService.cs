
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioDto>> GetAllAsync();
        Task<UsuarioDto?> GetByIdAsync(int id);
        Task<UsuarioDto> CreateAsync(UsuarioCreateDto dto);
    }
}
