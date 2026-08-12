using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UrbanSync.Application.Features.Job
{
    public sealed class UpdateJobDto
    {
        public int Id { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Result { get; set; }
    }
}
