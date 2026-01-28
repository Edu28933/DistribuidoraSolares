using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Usuario
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public int? SucursalId { get; set; } // NULL = SuperAdmin/Contador (ve todo)
    public string Nombre { get; set; } = string.Empty;
    public string UsuarioLogin { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties
    [NotMapped]
    public Rol? Rol { get; set; }
    [NotMapped]
    public Sucursal? Sucursal { get; set; }
}