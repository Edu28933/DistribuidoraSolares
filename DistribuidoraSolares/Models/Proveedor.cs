namespace DistribuidoraSolares.Models;

public class Proveedor
{
    public int ProveedorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Contacto { get; set; }
    public string Estado { get; set; } = "ACTIVO";
}