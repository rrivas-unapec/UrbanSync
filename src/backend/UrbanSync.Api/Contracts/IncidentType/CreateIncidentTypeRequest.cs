namespace UrbanSync.Api.Contracts.IncidentType
{
    public sealed class CreateIncidentTypeRequest
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int InstitutionId { get; set; }
    }
}
