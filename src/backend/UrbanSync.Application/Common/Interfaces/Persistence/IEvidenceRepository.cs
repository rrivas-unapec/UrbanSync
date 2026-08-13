using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Evidence;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IEvidenceRepository
    {
        Task<IReadOnlyList<EvidenceDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<EvidenceDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateEvidenceDto dto,
            CancellationToken cancellationToken = default);
    }
}
