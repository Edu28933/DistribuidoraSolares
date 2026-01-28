using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class CompraDetalle
{
    public int CompraDetalleId { get; set; }
    public int CompraId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Subtotal { get; set; }
    
    // Navigation properties
    [NotMapped]
    public Compra? Compra { get; set; }
    [NotMapped]
    public Producto? Producto { get; set; }
}