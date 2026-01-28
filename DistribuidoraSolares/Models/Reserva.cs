using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Reserva
{
    public int ReservaId { get; set; }
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int? SucursalId { get; set; } // Sucursal donde se hizo la reserva
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = "ACTIVA"; // ACTIVA / CONVERTIDA / CANCELADA / VENCIDA
    public string? Observacion { get; set; }
    
    // Navigation properties
    [NotMapped]
    public Cliente? Cliente { get; set; }
    [NotMapped]
    public Usuario? Usuario { get; set; }
    [NotMapped]
    public Sucursal? Sucursal { get; set; }
    [NotMapped]
    public List<ReservaDetalle>? Detalles { get; set; }
}