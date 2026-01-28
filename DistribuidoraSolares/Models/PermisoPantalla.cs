using System.ComponentModel.DataAnnotations.Schema;

namespace DistribuidoraSolares.Models;

public class PermisoPantalla
{
    public int PermisoPantallaId { get; set; }
    public int RolId { get; set; }
    public int PantallaId { get; set; }
    public bool PuedeVer { get; set; } = true;
    public bool PuedeCrear { get; set; } = false;
    public bool PuedeEditar { get; set; } = false;
    public bool PuedeEliminar { get; set; } = false;
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = "ACTIVO";
    
    // Navigation properties (marcadas como NotMapped para evitar problemas con SqlQueryRaw)
    [NotMapped]
    public Rol? Rol { get; set; }
    [NotMapped]
    public Pantalla? Pantalla { get; set; }
}
