namespace UrbanSync.Web.ViewModels;

public sealed class MyClaimsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<ClaimItemViewModel> Reclamaciones { get; set; } = [];
}

public sealed class CreateClaimPageViewModel
{
    public List<LocationOptionViewModel> Ubicaciones { get; set; } = [];
}
