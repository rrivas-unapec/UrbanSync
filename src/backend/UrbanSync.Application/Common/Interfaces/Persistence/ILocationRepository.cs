using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Location;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface ILocationRepository
    {
        Task<IReadOnlyList<LocationDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<LocationDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateLocationDto dto,
            CancellationToken cancellationToken = default);
    }
}
