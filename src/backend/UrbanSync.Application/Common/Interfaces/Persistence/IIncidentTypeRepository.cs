using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.IncidentType;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IIncidentTypeRepository
    {
        Task<IReadOnlyList<IncidentTypeDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IncidentTypeDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateIncidentTypeDto dto,
            CancellationToken cancellationToken = default);
    }
}
