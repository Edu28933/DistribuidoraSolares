namespace DistribuidoraSolares.Models;

public class CajaMovimiento
{
    public int MovimientoId { get; set; }
    public int CajaId { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public string Tipo { get; set; } = string.Empty; // INGRESO / EGRESO
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int? VentaId { get; set; }
    public int? CompraId { get; set; }
    public int UsuarioId { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties
    public Caja? Caja { get; set; }
    public Usuario? Usuario { get; set; }
}