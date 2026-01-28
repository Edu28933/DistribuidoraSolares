using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class Sucursal
{
    public int SucursalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
