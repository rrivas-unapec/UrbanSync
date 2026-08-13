namespace UrbanSync.Web.ViewModels;

public sealed class ClaimsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<ClaimItemViewModel> Reclamaciones { get; set; } = [];
}

public sealed class ClaimItemViewModel
{
    public int Id { get; set; }
    public string CitizenUserName { get; set; } = string.Empty;
    public string LocationAddress { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
