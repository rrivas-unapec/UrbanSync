using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Evidence
{
    public sealed class EvidenceDto
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public string EvidenceType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int UploadedByUserId { get; set; }

        public string UploadedByUserName { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }
    }
}
