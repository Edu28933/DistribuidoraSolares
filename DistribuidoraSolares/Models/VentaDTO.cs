namespace DistribuidoraSolares.Models;

// DTO para mapear resultados de stored procedures de ventas
public class VentaDTO
{
    public int VentaId { get; set; }
    public DateTime FechaHora { get; set; }
    public int? ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int MetodoPagoId { get; set; }
    public int? SucursalId { get; set; }
    public string TipoVenta { get; set; } = string.Empty;
    public decimal TotalBruto { get; set; }
    public decimal DescuentoManual { get; set; }
    public decimal TotalNeto { get; set; }
    public string? Observacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    
    // Campos adicionales del stored procedure
    [System.ComponentModel.DataAnnotations.Schema.Column("ClienteNombre")]
    public string? ClienteNombre { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.Column("UsuarioNombre")]
    public string? UsuarioNombre { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.Column("MetodoPagoNombre")]
    public string? MetodoPagoNombre { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.Column("SucursalNombre")]
    public string? SucursalNombre { get; set; }
}
