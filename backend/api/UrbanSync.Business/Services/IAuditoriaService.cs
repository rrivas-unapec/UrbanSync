using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public interface IAuditoriaService
    {
        Task<IEnumerable<AuditoriaDto>> GetLogsAsync(AuditoriaFilterDto filter);
        Task<AuditoriaDto?> GetLogByIdAsync(long id);
        Task<AuditoriaDto> RegisterLogAsync(AuditoriaCreateDto dto);
    }
}
