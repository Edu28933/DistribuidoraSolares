using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IClienteService
{
    Task<List<Cliente>> MostrarClientesAsync();
    Task<List<Cliente>> BuscarClientesAsync(string buscar);
    Task<int> CrearClienteAsync(Cliente cliente);
    Task<int> EditarClienteAsync(Cliente cliente);
    Task<int> EliminarClienteAsync(int clienteId);
}

public class ClienteService : IClienteService
{
    private readonly ApplicationDbContext _context;

    public ClienteService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Cliente>> MostrarClientesAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Cliente>("EXEC usp_clientes_mostrar")
            .ToListAsync();
    }

    public async Task<List<Cliente>> BuscarClientesAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Cliente>("EXEC usp_clientes_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearClienteAsync(Cliente cliente)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_clientes_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", cliente.Nombre));
        command.Parameters.Add(new SqlParameter("@Telefono", cliente.Telefono ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Nit", cliente.Nit ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Estado", cliente.Estado));

        var result = await command.ExecuteScalarAsync();
        
        // SCOPE_IDENTITY() puede devolver decimal, convertir a int
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarClienteAsync(Cliente cliente)
    {
        var parameters = new[]
        {
            new SqlParameter("@ClienteId", cliente.ClienteId),
            new SqlParameter("@Nombre", cliente.Nombre),
            new SqlParameter("@Telefono", cliente.Telefono ?? (object)DBNull.Value),
            new SqlParameter("@Nit", cliente.Nit ?? (object)DBNull.Value),
            new SqlParameter("@Estado", cliente.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_clientes_editar @ClienteId, @Nombre, @Telefono, @Nit, @Estado", parameters);
    }

    public async Task<int> EliminarClienteAsync(int clienteId)
    {
        var param = new SqlParameter("@ClienteId", clienteId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_clientes_eliminar @ClienteId", param);
    }
}