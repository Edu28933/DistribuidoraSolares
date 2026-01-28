namespace DistribuidoraSolares.Models;

public class VentaDetalle
{
    public int VentaDetalleId { get; set; }
    public int VentaId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioLista { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    
    // Navigation properties
    public Venta? Venta { get; set; }
    public Producto? Producto { get; set; }
}