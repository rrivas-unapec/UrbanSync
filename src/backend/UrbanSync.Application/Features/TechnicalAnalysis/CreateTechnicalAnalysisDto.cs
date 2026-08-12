using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.TechnicalAnalysis
{
    public sealed class CreateTechnicalAnalysisDto
    {
        public int IncidentId { get; set; }

        public int TechnicalUserId { get; set; }

        public string Diagnosis { get; set; } = string.Empty;

        public string? RecommendedActions { get; set; }
    }
}
