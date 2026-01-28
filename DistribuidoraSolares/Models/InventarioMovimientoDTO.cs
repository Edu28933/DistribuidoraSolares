using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

// DTO para mapear resultados de stored procedures de movimientos de inventario
public class InventarioMovimientoDTO
{
    public int InvMovId { get; set; }
    public int ProductoId { get; set; }
    public int? SucursalId { get; set; }
    public DateTime FechaHora { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public int? VentaId { get; set; }
    public int? CompraId { get; set; }
    public int? ReservaId { get; set; }
    public string? Descripcion { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = string.Empty;
    
    [Column("ProductoNombre")]
    public string? ProductoNombre { get; set; }
    
    [Column("UsuarioNombre")]
    public string? UsuarioNombre { get; set; }
    
    [Column("SucursalNombre")]
    public string? SucursalNombre { get; set; }
}
