using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IUsuarioService
{
    Task<List<Usuario>> MostrarUsuariosAsync();
    Task<List<Usuario>> BuscarUsuariosAsync(string buscar);
    Task<int> CrearUsuarioAsync(Usuario usuario);
    Task<int> EditarUsuarioAsync(Usuario usuario);
    Task<int> EliminarUsuarioAsync(int usuarioId);
}

public class UsuarioService : IUsuarioService
{
    private readonly ApplicationDbContext _context;

    public UsuarioService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Usuario>> MostrarUsuariosAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Usuario>("EXEC usp_usuarios_mostrar")
            .ToListAsync();
    }

    public async Task<List<Usuario>> BuscarUsuariosAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Usuario>("EXEC usp_usuarios_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearUsuarioAsync(Usuario usuario)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_usuarios_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@RolId", usuario.RolId));
        command.Parameters.Add(new SqlParameter("@Nombre", usuario.Nombre));
        command.Parameters.Add(new SqlParameter("@UsuarioLogin", usuario.UsuarioLogin));
        command.Parameters.Add(new SqlParameter("@PasswordHash", usuario.PasswordHash));
        command.Parameters.Add(new SqlParameter("@Estado", usuario.Estado));
        command.Parameters.Add(new SqlParameter("@SucursalId", usuario.SucursalId ?? (object)DBNull.Value));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarUsuarioAsync(Usuario usuario)
    {
        var parameters = new[]
        {
            new SqlParameter("@UsuarioId", usuario.UsuarioId),
            new SqlParameter("@RolId", usuario.RolId),
            new SqlParameter("@Nombre", usuario.Nombre),
            new SqlParameter("@UsuarioLogin", usuario.UsuarioLogin),
            new SqlParameter("@PasswordHash", usuario.PasswordHash ?? (object)DBNull.Value),
            new SqlParameter("@Estado", usuario.Estado),
            new SqlParameter("@SucursalId", usuario.SucursalId ?? (object)DBNull.Value)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_usuarios_editar @UsuarioId, @RolId, @Nombre, @UsuarioLogin, @PasswordHash, @Estado, @SucursalId", parameters);
    }

    public async Task<int> EliminarUsuarioAsync(int usuarioId)
    {
        var param = new SqlParameter("@UsuarioId", usuarioId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_usuarios_eliminar @UsuarioId", param);
    }
}
