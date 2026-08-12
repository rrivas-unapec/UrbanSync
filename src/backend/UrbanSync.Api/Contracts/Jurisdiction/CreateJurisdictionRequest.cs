namespace UrbanSync.Api.Contracts.Jurisdiction
{
    public sealed class CreateJurisdictionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Level { get; set; } = string.Empty;

        public int? ParentJurisdictionId { get; set; }
    }
}
