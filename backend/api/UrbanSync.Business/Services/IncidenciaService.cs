using UrbanSync.DataAccess.Repositories;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public class IncidenciaService : IIncidenciaService
    {
        private readonly IIncidenciaRepository _repository;

        public IncidenciaService(IIncidenciaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<IncidenciaDto>> GetAllIncidenciasAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IncidenciaDto?> GetIncidenciaByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IncidenciaDto> CreateIncidenciaAsync(IncidenciaCreateDto dto)
        {
            string codigoCaso = $"INC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4].ToUpper()}";
            int newId = await _repository.CreateAsync(dto, codigoCaso);

            var result = await _repository.GetByIdAsync(newId);
            return result!;
        }

        public async Task<bool> UpdateEstadoAsync(int id, IncidenciaEstadoUpdateDto dto)
        {
            return await _repository.UpdateEstadoAsync(id, dto.Estado, dto.InstitucionAsignadaId);
        }
    }
}
