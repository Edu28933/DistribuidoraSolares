using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Services;

public interface IProveedorService
{
    Task<List<Proveedor>> MostrarProveedoresAsync();
    Task<List<Proveedor>> BuscarProveedoresAsync(string buscar);
    Task<int> CrearProveedorAsync(Proveedor proveedor);
    Task<int> EditarProveedorAsync(Proveedor proveedor);
    Task<int> EliminarProveedorAsync(int proveedorId);
}

public class ProveedorService : IProveedorService
{
    private readonly ApplicationDbContext _context;

    public ProveedorService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Proveedor>> MostrarProveedoresAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Proveedor>("EXEC usp_proveedores_mostrar")
            .ToListAsync();
    }

    public async Task<List<Proveedor>> BuscarProveedoresAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Proveedor>("EXEC usp_proveedores_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearProveedorAsync(Proveedor proveedor)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new Microsoft.Data.SqlClient.SqlCommand("usp_proveedores_crear", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", proveedor.Nombre));
        command.Parameters.Add(new SqlParameter("@Contacto", proveedor.Contacto ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Estado", proveedor.Estado));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarProveedorAsync(Proveedor proveedor)
    {
        var parameters = new[]
        {
            new SqlParameter("@ProveedorId", proveedor.ProveedorId),
            new SqlParameter("@Nombre", proveedor.Nombre),
            new SqlParameter("@Contacto", proveedor.Contacto ?? (object)DBNull.Value),
            new SqlParameter("@Estado", proveedor.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_proveedores_editar @ProveedorId, @Nombre, @Contacto, @Estado", parameters);
    }

    public async Task<int> EliminarProveedorAsync(int proveedorId)
    {
        var param = new SqlParameter("@ProveedorId", proveedorId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_proveedores_eliminar @ProveedorId", param);
    }
}
