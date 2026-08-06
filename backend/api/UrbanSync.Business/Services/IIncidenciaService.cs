using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public interface IIncidenciaService
    {
        Task<IEnumerable<IncidenciaDto>> GetAllIncidenciasAsync();
        Task<IncidenciaDto?> GetIncidenciaByIdAsync(int id);
        Task<IncidenciaDto> CreateIncidenciaAsync(IncidenciaCreateDto dto);
        Task<bool> UpdateEstadoAsync(int id, IncidenciaEstadoUpdateDto dto);
    }
}
