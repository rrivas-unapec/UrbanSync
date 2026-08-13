using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Report
{
    public sealed class CreateReportDto
    {
        public int IncidentId { get; set; }

        public int? JobId { get; set; }

        public int GeneratedByUserId { get; set; }

        public string? Content { get; set; }

        public string? FilePath { get; set; }
    }
}
