namespace DistribuidoraSolares.Models;

public class TVP_VentaDetalle
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioLista { get; set; }
    public decimal PrecioUnitario { get; set; }
}