using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Job;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IJobRepository
    {
        Task<IReadOnlyList<JobDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<JobDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateJobDto dto,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            UpdateJobDto dto,
            CancellationToken cancellationToken = default);
    }
}
