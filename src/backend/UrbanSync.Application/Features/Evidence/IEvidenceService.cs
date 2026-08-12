using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Evidence
{
    public interface IEvidenceService
    {
        Task<IReadOnlyList<EvidenceDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<EvidenceDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<EvidenceDto> CreateAsync(
            CreateEvidenceDto dto,
            CancellationToken cancellationToken = default);
    }
}
