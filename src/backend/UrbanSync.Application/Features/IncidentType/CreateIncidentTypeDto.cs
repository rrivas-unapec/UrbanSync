using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.IncidentType
{
    public sealed class CreateIncidentTypeDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int InstitutionId { get; set; }
    }
}
