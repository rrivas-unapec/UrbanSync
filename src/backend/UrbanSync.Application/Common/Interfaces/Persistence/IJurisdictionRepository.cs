using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Jurisdiction;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IJurisdictionRepository
    {
        Task<IReadOnlyList<JurisdictionDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<JurisdictionDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateJurisdictionDto dto,
            CancellationToken cancellationToken = default);
    }
}
