using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Report
{
    public interface IReportService
    {
        Task<IReadOnlyList<ReportDto>> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<ReportDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<ReportDto> CreateAsync(
            CreateReportDto dto,
            CancellationToken cancellationToken = default);
    }
}
