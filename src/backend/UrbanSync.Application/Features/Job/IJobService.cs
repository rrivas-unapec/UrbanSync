using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Job
{
    public interface IJobService
    {
        Task<IReadOnlyList<JobDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<JobDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<JobDto> CreateAsync(
            CreateJobDto dto,
            CancellationToken cancellationToken = default);

        Task<JobDto?> UpdateAsync(
            UpdateJobDto dto,
            CancellationToken cancellationToken = default);
    }
}
