namespace DistribuidoraSolares.Models;

public class ReservaDetalle
{
    public int ReservaDetalleId { get; set; }
    public int ReservaId { get; set; }
    public int ProductoId { get; set; }
    public int CantidadReservada { get; set; }
    public string Estado { get; set; } = "ACTIVO"; // ACTIVO / CANCELADO / OCUPADO
    
    // Navigation properties
    public Reserva? Reserva { get; set; }
    public Producto? Producto { get; set; }
}