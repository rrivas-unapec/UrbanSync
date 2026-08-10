namespace UrbanSync.Api.Contracts.Assets
{
    public sealed class AssetResponse
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int JurisdictionId { get; set; }

        public string JurisdictionName { get; set; } = string.Empty;

        public DateTime? InstallationDate { get; set; }

        public bool IsActive { get; set; }
    }
}
