namespace UrbanSync.Web.ViewModels;

public sealed class JurisdictionsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<JurisdictionItemViewModel> Jurisdicciones { get; set; } = [];
    public List<JurisdictionOptionViewModel> OpcionesPadre { get; set; } = [];
}

public sealed class JurisdictionItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string? ParentJurisdictionName { get; set; }
    public bool IsActive { get; set; }
}

public sealed class JurisdictionOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
