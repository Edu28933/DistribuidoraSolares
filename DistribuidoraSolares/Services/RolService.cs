using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IRolService
{
    Task<List<Rol>> MostrarRolesAsync();
    Task<List<Rol>> BuscarRolesAsync(string buscar);
    Task<int> CrearRolAsync(Rol rol);
    Task<int> EditarRolAsync(Rol rol);
    Task<int> EliminarRolAsync(int rolId);
}

public class RolService : IRolService
{
    private readonly ApplicationDbContext _context;

    public RolService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rol>> MostrarRolesAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Rol>("EXEC usp_roles_mostrar")
            .ToListAsync();
    }

    public async Task<List<Rol>> BuscarRolesAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Rol>("EXEC usp_roles_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearRolAsync(Rol rol)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_roles_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", rol.Nombre));
        command.Parameters.Add(new SqlParameter("@Estado", rol.Estado));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarRolAsync(Rol rol)
    {
        var parameters = new[]
        {
            new SqlParameter("@RolId", rol.RolId),
            new SqlParameter("@Nombre", rol.Nombre),
            new SqlParameter("@Estado", rol.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_roles_editar @RolId, @Nombre, @Estado", parameters);
    }

    public async Task<int> EliminarRolAsync(int rolId)
    {
        var param = new SqlParameter("@RolId", rolId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_roles_eliminar @RolId", param);
    }
}
