using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Institution
{
    public interface IInstitutionService
    {
        Task<IReadOnlyList<InstitutionDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<InstitutionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<InstitutionDto> CreateAsync(
            CreateInstitutionDto dto,
            CancellationToken cancellationToken = default);
    }
}
