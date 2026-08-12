using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Institution;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IInstitutionRepository
    {
        Task<IReadOnlyList<InstitutionDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<InstitutionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateInstitutionDto dto,
            CancellationToken cancellationToken = default);
    }
}
