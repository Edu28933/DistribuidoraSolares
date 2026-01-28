using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Compra
{
    public int CompraId { get; set; }
    public int ProveedorId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    public int UsuarioId { get; set; }
    public int? SucursalId { get; set; } // Sucursal donde se recibió la compra
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties
    [NotMapped]
    public Proveedor? Proveedor { get; set; }
    [NotMapped]
    public Usuario? Usuario { get; set; }
    [NotMapped]
    public Sucursal? Sucursal { get; set; }
    [NotMapped]
    public List<CompraDetalle>? Detalles { get; set; }
}