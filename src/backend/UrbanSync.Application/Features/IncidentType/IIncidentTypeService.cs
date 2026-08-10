using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.IncidentType
{
    public interface IIncidentTypeService
    {
        Task<IReadOnlyList<IncidentTypeDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<IncidentTypeDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IncidentTypeDto> CreateAsync(
            CreateIncidentTypeDto dto,
            CancellationToken cancellationToken = default);
    }
}
