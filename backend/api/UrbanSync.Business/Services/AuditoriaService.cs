using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.DataAccess.Repositories;
using UrbanSync.Domain.DTOs;

namespace UrbanSync.Business.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IAuditoriaRepository _repository;

        public AuditoriaService(IAuditoriaRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AuditoriaDto>> GetLogsAsync(AuditoriaFilterDto filter)
        {
            return await _repository.GetAllAsync(filter);
        }

        public async Task<AuditoriaDto?> GetLogByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<AuditoriaDto> RegisterLogAsync(AuditoriaCreateDto dto)
        {
            var newId = await _repository.CreateAsync(dto);
            var result = await _repository.GetByIdAsync(newId);
            return result!;
        }
    }
}