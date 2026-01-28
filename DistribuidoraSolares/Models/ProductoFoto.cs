using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class ProductoFoto
{
    public int FotoId { get; set; }
    public int ProductoId { get; set; }
    public string UrlFoto { get; set; } = string.Empty;
    public bool EsPrincipal { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public int UsuarioId { get; set; }
    
    // Navigation properties
    [NotMapped]
    public Producto? Producto { get; set; }
    [NotMapped]
    public Usuario? Usuario { get; set; }
}