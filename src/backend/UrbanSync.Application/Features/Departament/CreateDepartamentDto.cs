using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Departament
{
    public sealed class CreateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;

        public int? JurisdictionId { get; set; }
    }
}
