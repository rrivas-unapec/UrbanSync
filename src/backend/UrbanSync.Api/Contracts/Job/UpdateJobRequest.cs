namespace UrbanSync.Api.Contracts.Job
{
    public sealed class UpdateJobRequest
    {
        public string Status { get; set; } = string.Empty;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Result { get; set; }
    }   
}
