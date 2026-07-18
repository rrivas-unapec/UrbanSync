using UrbanSync.Domain.Entities;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<int> CreateAsync(Usuario usuario);
        Task<bool> ToggleStatusAsync(int id);
    }
}
