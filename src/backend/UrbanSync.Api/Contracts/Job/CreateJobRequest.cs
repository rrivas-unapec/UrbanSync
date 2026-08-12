namespace UrbanSync.Api.Contracts.Job
{
    public sealed class CreateJobRequest
    {
        public int IncidentId { get; set; }

        public int AssignedUserId { get; set; }

        public string JobDescription { get; set; } = string.Empty;

        public string Status { get; set; } = "Pendiente";

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Result { get; set; }
    }
}
