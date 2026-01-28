namespace DistribuidoraSolares.Models;

public class Pantalla
{
    public int PantallaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Controlador { get; set; } = string.Empty;
    public string? Accion { get; set; }
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "ACTIVO";
}
