namespace DistribuidoraSolares.Models;

public class ReporteCliente
{
    public Cliente Cliente { get; set; } = new();
    public int CantidadVentas { get; set; }
    public decimal TotalCompras { get; set; }
}
