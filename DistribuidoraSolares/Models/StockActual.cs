using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class StockActual
{
    public int ProductoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int StockMinimo { get; set; }
    
    [Column("StockActual")]
    public int Stock { get; set; }
}