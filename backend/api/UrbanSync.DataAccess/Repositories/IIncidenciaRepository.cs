using UrbanSync.Domain.DTOs;

namespace UrbanSync.DataAccess.Repositories
{
    public interface IIncidenciaRepository
    {
        Task<IEnumerable<IncidenciaDto>> GetAllAsync();
        Task<IncidenciaDto?> GetByIdAsync(int id);
        Task<int> CreateAsync(IncidenciaCreateDto dto, string codigoCaso);
        Task<bool> UpdateEstadoAsync(int id, string estado, int? institucionId);
    }
}
