using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class InventarioMovimiento
{
    public int InvMovId { get; set; }
    public int ProductoId { get; set; }
    public int? SucursalId { get; set; } // Sucursal del movimiento (CRÍTICO para stock por sucursal)
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public string Tipo { get; set; } = string.Empty; // COMPRA / VENTA / PERDIDA / AJUSTE / RESERVA / LIBERACION_RESERVA
    public int Cantidad { get; set; }
    public decimal? CostoUnitario { get; set; }
    public int? VentaId { get; set; }
    public int? CompraId { get; set; }
    public int? ReservaId { get; set; }
    public string? Descripcion { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties
    [NotMapped]
    public Producto? Producto { get; set; }
    [NotMapped]
    public Usuario? Usuario { get; set; }
    [NotMapped]
    public Sucursal? Sucursal { get; set; }
}