namespace UrbanSync.Api.Contracts.Job
{
    public sealed class JobResponse
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }

        public int AssignedUserId { get; set; }

        public string AssignedUserName { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Result { get; set; }
    }
}
