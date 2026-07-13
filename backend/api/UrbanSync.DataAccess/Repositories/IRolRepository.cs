
using UrbanSync.Domain.Entities;

namespace UrbanSync.DataAccess.Repositories
{
    public interface IRolRepository
    {
        Task<IEnumerable<Rol>> GetAllAsync();
        Task<Rol?> GetByIdAsync(int id);
        Task<int> CreateAsync(Rol rol);
    }
}
