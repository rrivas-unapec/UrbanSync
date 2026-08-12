using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Institution
{
    public sealed class InstitutionDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string InstitutionType { get; set; } = string.Empty;

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public bool IsActive { get; set; }
    }
}
