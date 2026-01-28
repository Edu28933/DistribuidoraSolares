namespace DistribuidoraSolares.Models;

public class Cliente
{
    public int ClienteId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Nit { get; set; }
    public string Estado { get; set; } = "ACTIVO";
}