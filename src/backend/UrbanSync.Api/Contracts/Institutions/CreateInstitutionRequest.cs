namespace UrbanSync.Api.Contracts.Institutions
{
    public sealed class CreateInstitutionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string InstitutionType { get; set; } = string.Empty;

        public string? ContactEmail { get; set; }

        public string? ContactPhone { get; set; }
    }
}
