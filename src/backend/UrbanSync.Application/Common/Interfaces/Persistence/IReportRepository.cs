using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.Report;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface IReportRepository
    {
        Task<IReadOnlyList<ReportDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<ReportDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateReportDto dto,
            CancellationToken cancellationToken = default);
    }
}
