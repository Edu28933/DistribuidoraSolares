namespace DistribuidoraSolares.Models;

public class MetodoPago
{
    public int MetodoPagoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Estado { get; set; } = "ACTIVO";
}