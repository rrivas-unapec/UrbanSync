using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.TechnicalAnalysis
{
    public interface ITechnicalAnalysisService
    {
        Task<TechnicalAnalysisDto?> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<TechnicalAnalysisDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<TechnicalAnalysisDto> CreateAsync(
            CreateTechnicalAnalysisDto dto,
            CancellationToken cancellationToken = default);
    }
}
