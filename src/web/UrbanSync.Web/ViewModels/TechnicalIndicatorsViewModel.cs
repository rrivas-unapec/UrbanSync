namespace UrbanSync.Web.ViewModels;

public sealed class TechnicalIndicatorsViewModel
{
    public bool DatosDisponibles { get; set; } = true;
    public int Total { get; set; }
    public List<IndicatorCountViewModel> PorEstado { get; set; } = [];
    public List<IndicatorCountViewModel> PorPrioridad { get; set; } = [];
}

public sealed class IndicatorCountViewModel
{
    public string Clave { get; set; } = string.Empty;
    public int Total { get; set; }
}
