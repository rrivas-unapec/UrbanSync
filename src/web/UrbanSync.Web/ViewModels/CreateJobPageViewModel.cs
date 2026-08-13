namespace UrbanSync.Web.ViewModels;

public sealed class CreateJobPageViewModel
{
    public List<IncidentOptionViewModel> Incidencias { get; set; } = [];
    public List<UserOptionViewModel> Usuarios { get; set; } = [];
}

public sealed class IncidentOptionViewModel
{
    public int Id { get; set; }
    public string CodigoCaso { get; set; } = string.Empty;
    public string TipoIncidencia { get; set; } = string.Empty;
}

public sealed class UserOptionViewModel
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}
