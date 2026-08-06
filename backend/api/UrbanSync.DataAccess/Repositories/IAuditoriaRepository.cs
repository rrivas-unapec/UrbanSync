using UrbanSync.Domain.DTOs;

namespace UrbanSync.DataAccess.Repositories
{
    public interface IAuditoriaRepository
    {
        Task<IEnumerable<AuditoriaDto>> GetAllAsync(AuditoriaFilterDto filter);
        Task<AuditoriaDto?> GetByIdAsync(long id);
        Task<long> CreateAsync(AuditoriaCreateDto dto);
    }
}
