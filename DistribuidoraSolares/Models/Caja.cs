namespace DistribuidoraSolares.Models;

public class Caja
{
    public int CajaId { get; set; }
    public int? SucursalId { get; set; } // Sucursal a la que pertenece la caja
    public DateTime FechaApertura { get; set; }
    public decimal SaldoInicial { get; set; }
    public DateTime? FechaCierre { get; set; }
    public decimal? SaldoFinal { get; set; }
    public string Estado { get; set; } = "CERRADA";
}