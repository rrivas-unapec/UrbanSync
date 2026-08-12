using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Report
{
    public sealed class ReportDto
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public int? JobId { get; set; }

        public int GeneratedByUserId { get; set; }

        public string GeneratedByUserName { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? FilePath { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
