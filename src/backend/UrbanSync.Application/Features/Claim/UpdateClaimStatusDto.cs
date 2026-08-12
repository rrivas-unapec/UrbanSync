using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Claim
{
    public sealed class UpdateClaimStatusDto
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
