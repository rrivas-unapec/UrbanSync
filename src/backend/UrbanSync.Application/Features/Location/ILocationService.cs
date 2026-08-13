using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Location
{
    public interface ILocationService
    {
        Task<IReadOnlyList<LocationDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<LocationDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<LocationDto> CreateAsync(
            CreateLocationDto dto,
            CancellationToken cancellationToken = default);
    }
}
