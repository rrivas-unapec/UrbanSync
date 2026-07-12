namespace UrbanSync.Web.ViewModels;

public sealed class RutasViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public List<CuadrillaViewModel> Cuadrillas { get; set; } = [];
}

public sealed class CuadrillaViewModel
{
    public string Tecnico { get; set; } = string.Empty;
    public int TotalOrdenes { get; set; }
    public int Pendientes { get; set; }
    public int EnProgreso { get; set; }
    public int Finalizadas { get; set; }
    public string Estado { get; set; } = string.Empty;
}
