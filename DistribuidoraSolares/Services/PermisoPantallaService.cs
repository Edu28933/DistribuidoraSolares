using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IPermisoPantallaService
{
    Task<List<PermisoPantalla>> ObtenerPermisosPorRolAsync(int rolId);
    Task<List<Pantalla>> ObtenerPantallasAsync();
    Task<int> GuardarPermisoAsync(PermisoPantalla permiso);
    Task<bool> TienePermisoAsync(int usuarioId, string controlador, string accion, string tipoPermiso);
}

public class PermisoPantallaService : IPermisoPantallaService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsuarioService _usuarioService;

    public PermisoPantallaService(ApplicationDbContext context, IUsuarioService usuarioService)
    {
        _context = context;
        _usuarioService = usuarioService;
    }

    public async Task<List<PermisoPantalla>> ObtenerPermisosPorRolAsync(int rolId)
    {
        var param = new SqlParameter("@RolId", rolId);
        return await _context.Database
            .SqlQueryRaw<PermisoPantalla>("EXEC usp_permisos_pantalla_obtener_por_rol @RolId", param)
            .ToListAsync();
    }

    /// <summary>
    /// Pantallas para la UI de permisos: solo una por módulo (Accion = Index).
    /// Las columnas Puede Ver/Crear/Editar/Eliminar se aplican a ese módulo.
    /// </summary>
    public async Task<List<Pantalla>> ObtenerPantallasAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Pantalla>("SELECT * FROM tbl_pantallas WHERE Estado = 'ACTIVO' AND (Accion = 'Index' OR Accion IS NULL) ORDER BY Nombre")
            .ToListAsync();
    }

    public async Task<int> GuardarPermisoAsync(PermisoPantalla permiso)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_permisos_pantalla_guardar", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@RolId", permiso.RolId));
        command.Parameters.Add(new SqlParameter("@PantallaId", permiso.PantallaId));
        command.Parameters.Add(new SqlParameter("@PuedeVer", permiso.PuedeVer));
        command.Parameters.Add(new SqlParameter("@PuedeCrear", permiso.PuedeCrear));
        command.Parameters.Add(new SqlParameter("@PuedeEditar", permiso.PuedeEditar));
        command.Parameters.Add(new SqlParameter("@PuedeEliminar", permiso.PuedeEliminar));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    /// <summary>
    /// Verifica permiso: un solo registro por módulo (pantalla Index). La acción solicitada
    /// se mapea al tipo Ver/Crear/Editar/Eliminar y se revisa esa columna del permiso.
    /// </summary>
    public async Task<bool> TienePermisoAsync(int usuarioId, string controlador, string accion, string tipoPermiso)
    {
        var usuarios = await _usuarioService.MostrarUsuariosAsync();
        var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == usuarioId);
        if (usuario == null)
        {
            System.Diagnostics.Debug.WriteLine($"[PERMISOS] Usuario {usuarioId} no encontrado");
            return false;
        }

        var permisos = await ObtenerPermisosPorRolAsync(usuario.RolId);
        var pantallas = await ObtenerPantallasAsync();

        // Una fila por módulo: se busca la pantalla del controlador (Index)
        var permiso = permisos.FirstOrDefault(p =>
        {
            var pantalla = pantallas.FirstOrDefault(pa => pa.PantallaId == p.PantallaId);
            return pantalla != null && pantalla.Controlador.Equals(controlador, StringComparison.OrdinalIgnoreCase);
        });

        if (permiso == null)
        {
            System.Diagnostics.Debug.WriteLine($"[PERMISOS] No se encontró permiso para controlador {controlador}, permitiendo por defecto");
            return true;
        }

        var resultado = tipoPermiso switch
        {
            "Ver" => permiso.PuedeVer,
            "Crear" => permiso.PuedeCrear,
            "Editar" => permiso.PuedeEditar,
            "Eliminar" => permiso.PuedeEliminar,
            _ => permiso.PuedeVer
        };

        System.Diagnostics.Debug.WriteLine($"[PERMISOS] Controlador={controlador}, Accion={accion}, TipoPermiso={tipoPermiso} -> {resultado}");
        return resultado;
    }
}
