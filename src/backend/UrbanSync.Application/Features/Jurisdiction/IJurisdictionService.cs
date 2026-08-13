using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Jurisdiction
{
    public interface IJurisdictionService
    {
        Task<IReadOnlyList<JurisdictionDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<JurisdictionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<JurisdictionDto> CreateAsync(
            CreateJurisdictionDto dto,
            CancellationToken cancellationToken = default);
    }
}
