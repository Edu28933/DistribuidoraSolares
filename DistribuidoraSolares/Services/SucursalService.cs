using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Services;

public interface ISucursalService
{
    Task<List<Sucursal>> MostrarSucursalesAsync();
    Task<List<Sucursal>> BuscarSucursalesAsync(string buscar);
    Task<int> CrearSucursalAsync(Sucursal sucursal);
    Task<int> EditarSucursalAsync(Sucursal sucursal);
    Task<int> EliminarSucursalAsync(int sucursalId);
    Task<Sucursal?> ObtenerSucursalPorIdAsync(int sucursalId);
}

public class SucursalService : ISucursalService
{
    private readonly ApplicationDbContext _context;

    public SucursalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sucursal>> MostrarSucursalesAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Sucursal>("EXEC usp_sucursales_mostrar")
            .ToListAsync();
    }

    public async Task<List<Sucursal>> BuscarSucursalesAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Sucursal>("EXEC usp_sucursales_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearSucursalAsync(Sucursal sucursal)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_sucursales_crear", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", sucursal.Nombre));
        command.Parameters.Add(new SqlParameter("@Codigo", sucursal.Codigo ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Direccion", sucursal.Direccion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Telefono", sucursal.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Estado", sucursal.Estado));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarSucursalAsync(Sucursal sucursal)
    {
        var parameters = new[]
        {
            new SqlParameter("@SucursalId", sucursal.SucursalId),
            new SqlParameter("@Nombre", sucursal.Nombre),
            new SqlParameter("@Codigo", sucursal.Codigo ?? (object)DBNull.Value),
            new SqlParameter("@Direccion", sucursal.Direccion ?? (object)DBNull.Value),
            new SqlParameter("@Telefono", sucursal.Telefono ?? (object)DBNull.Value),
            new SqlParameter("@Estado", sucursal.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_sucursales_editar @SucursalId, @Nombre, @Codigo, @Direccion, @Telefono, @Estado", parameters);
    }

    public async Task<int> EliminarSucursalAsync(int sucursalId)
    {
        var param = new SqlParameter("@SucursalId", sucursalId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_sucursales_eliminar @SucursalId", param);
    }

    public async Task<Sucursal?> ObtenerSucursalPorIdAsync(int sucursalId)
    {
        var param = new SqlParameter("@SucursalId", sucursalId);
        var sucursales = await _context.Database
            .SqlQueryRaw<Sucursal>("EXEC usp_sucursales_obtener_por_id @SucursalId", param)
            .ToListAsync();
        
        return sucursales.FirstOrDefault();
    }
}

// Helper para obtener SucursalId según el rol del usuario
public static class SucursalHelper
{
    /// <summary>
    /// Obtiene el SucursalId para filtrar datos según el rol del usuario.
    /// SuperAdmin y Contador retornan null (ven todo).
    /// Admin y Vendedor retornan su SucursalId asignado.
    /// </summary>
    public static int? ObtenerSucursalIdParaFiltro(string? rolNombre, int? sucursalIdUsuario)
    {
        if (string.IsNullOrEmpty(rolNombre))
            return null;

        // Roles que ven todo (sin filtro)
        if (rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
            rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase) ||
            rolNombre.Equals("Super Admin", StringComparison.OrdinalIgnoreCase) ||
            rolNombre.Equals("Contador", StringComparison.OrdinalIgnoreCase) ||
            rolNombre.Equals("CONTADOR", StringComparison.OrdinalIgnoreCase))
        {
            return null; // Sin filtro = ve todo
        }

        // Admin y Vendedor ven solo su sucursal
        return sucursalIdUsuario;
    }
}
