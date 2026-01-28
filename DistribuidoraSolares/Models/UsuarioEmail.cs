using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class UsuarioEmail
{
    public int UsuarioEmailId { get; set; }
    public int UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation property (marked as NotMapped for SqlQueryRaw compatibility)
    [NotMapped]
    public Usuario? Usuario { get; set; }
}
