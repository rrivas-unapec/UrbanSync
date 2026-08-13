namespace UrbanSync.Web.ViewModels;

public sealed class DepartmentsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<DepartmentItemViewModel> Departamentos { get; set; } = [];
    public List<JurisdictionOptionViewModel> OpcionesJurisdiccion { get; set; } = [];
}

public sealed class DepartmentItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? JurisdictionName { get; set; }
    public bool IsActive { get; set; }
}
