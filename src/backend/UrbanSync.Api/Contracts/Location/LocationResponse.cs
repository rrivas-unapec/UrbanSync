namespace UrbanSync.Api.Contracts.Location
{
    public sealed class LocationResponse
    {
        public int Id { get; set; }

        public string Address { get; set; } = string.Empty;

        public string? Reference { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public int JurisdictionId { get; set; }

        public string JurisdictionName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
