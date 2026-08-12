namespace UrbanSync.Api.Contracts.Institutions
{
    public sealed class InstitutionResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string InstitutionType { get; set; } = string.Empty;

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }

        public bool IsActive { get; set; }
    }
}
