using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Producto
{
    public int ProductoId { get; set; }
    public int CategoriaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public decimal CostoReferencia { get; set; }
    public int StockMinimo { get; set; }
    public string Estado { get; set; } = "DISPONIBLE";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    // Navigation properties
    [NotMapped]
    public Categoria? Categoria { get; set; }
}