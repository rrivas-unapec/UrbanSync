namespace UrbanSync.Api.Contracts.Claim
{
    public sealed class CreateClaimRequest
    {
        public int CitizenUserId { get; set; }

        public int LocationId { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
