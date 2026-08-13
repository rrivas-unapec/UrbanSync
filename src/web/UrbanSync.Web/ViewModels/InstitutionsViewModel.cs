namespace UrbanSync.Web.ViewModels;

public sealed class InstitutionsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<InstitutionItemViewModel> Instituciones { get; set; } = [];
}

public sealed class InstitutionItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InstitutionType { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
}
