
using UrbanSync.Domain.Entities;

namespace UrbanSync.DataAccess.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
        Task<int> CreateAsync(Usuario usuario);
    }
}
