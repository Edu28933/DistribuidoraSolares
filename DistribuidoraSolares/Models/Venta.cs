using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Venta
{
    public int VentaId { get; set; }
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public int? ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int MetodoPagoId { get; set; }
    public int? SucursalId { get; set; } // Sucursal donde se realizó la venta
    public string TipoVenta { get; set; } = "NORMAL"; // NORMAL / PERDIDA
    public decimal TotalBruto { get; set; }
    public decimal DescuentoManual { get; set; }
    public decimal TotalNeto { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties
    [NotMapped]
    public Cliente? Cliente { get; set; }
    [NotMapped]
    public Usuario? Usuario { get; set; }
    [NotMapped]
    public MetodoPago? MetodoPago { get; set; }
    [NotMapped]
    public Sucursal? Sucursal { get; set; }
    [NotMapped]
    public List<VentaDetalle>? Detalles { get; set; }
}