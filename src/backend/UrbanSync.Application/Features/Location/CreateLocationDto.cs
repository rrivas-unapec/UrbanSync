using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Location
{
    public sealed class CreateLocationDto
    {
        public string Address { get; set; } = string.Empty;

        public string? Reference { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int JurisdictionId { get; set; }
    }
}
