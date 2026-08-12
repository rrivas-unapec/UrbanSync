using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Claim;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IClaimRepository
    {
        Task<IReadOnlyList<ClaimDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ClaimDto>> GetByCitizenIdAsync(
            int citizenUserId,
            CancellationToken cancellationToken = default);

        Task<ClaimDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateClaimDto dto,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateStatusAsync(
            UpdateClaimStatusDto dto,
            CancellationToken cancellationToken = default);
    }
}
