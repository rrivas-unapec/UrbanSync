using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Claim
{
    public interface IClaimService
    {
        Task<IReadOnlyList<ClaimDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ClaimDto>> GetByCitizenIdAsync(
            int citizenUserId,
            CancellationToken cancellationToken = default);

        Task<ClaimDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ClaimDto> CreateAsync(
            CreateClaimDto dto,
            CancellationToken cancellationToken = default);

        Task<ClaimDto?> UpdateStatusAsync(
            UpdateClaimStatusDto dto,
            CancellationToken cancellationToken = default);
    }
}
