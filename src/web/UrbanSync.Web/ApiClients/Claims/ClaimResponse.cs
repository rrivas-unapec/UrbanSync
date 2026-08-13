namespace UrbanSync.Web.ApiClients.Claims;

public sealed class ClaimResponse
{
    public int Id { get; set; }

    public int CitizenUserId { get; set; }

    public string CitizenUserName { get; set; } = string.Empty;

    public int LocationId { get; set; }

    public string LocationAddress { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
