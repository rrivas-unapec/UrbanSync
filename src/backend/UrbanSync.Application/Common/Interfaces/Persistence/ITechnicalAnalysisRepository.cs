using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrbanSync.Application.Features.TechnicalAnalysis;

namespace UrbanSync.Application.Common.Interfaces.Persistence
{
    public interface ITechnicalAnalysisRepository
    {
        Task<TechnicalAnalysisDto?> GetByIncidentIdAsync(
            int incidentId,
            CancellationToken cancellationToken = default);

        Task<TechnicalAnalysisDto?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<int> CreateAsync(
            CreateTechnicalAnalysisDto dto,
            CancellationToken cancellationToken = default);
    }
}
