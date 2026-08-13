using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Claim
{
    public sealed class CreateClaimDto
    {
        public int CitizenUserId { get; set; }

        public int LocationId { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
