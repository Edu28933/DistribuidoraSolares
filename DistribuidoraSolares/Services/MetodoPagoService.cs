using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IMetodoPagoService
{
    Task<List<MetodoPago>> MostrarMetodosPagoAsync();
    Task<List<MetodoPago>> BuscarMetodosPagoAsync(string buscar);
    Task<int> CrearMetodoPagoAsync(MetodoPago metodoPago);
    Task<int> EditarMetodoPagoAsync(MetodoPago metodoPago);
    Task<int> EliminarMetodoPagoAsync(int metodoPagoId);
}

public class MetodoPagoService : IMetodoPagoService
{
    private readonly ApplicationDbContext _context;

    public MetodoPagoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MetodoPago>> MostrarMetodosPagoAsync()
    {
        return await _context.Database
            .SqlQueryRaw<MetodoPago>("EXEC usp_metodos_pago_mostrar")
            .ToListAsync();
    }

    public async Task<List<MetodoPago>> BuscarMetodosPagoAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<MetodoPago>("EXEC usp_metodos_pago_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearMetodoPagoAsync(MetodoPago metodoPago)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_metodos_pago_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", metodoPago.Nombre));
        command.Parameters.Add(new SqlParameter("@Estado", metodoPago.Estado));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarMetodoPagoAsync(MetodoPago metodoPago)
    {
        var parameters = new[]
        {
            new SqlParameter("@MetodoPagoId", metodoPago.MetodoPagoId),
            new SqlParameter("@Nombre", metodoPago.Nombre),
            new SqlParameter("@Estado", metodoPago.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_metodos_pago_editar @MetodoPagoId, @Nombre, @Estado", parameters);
    }

    public async Task<int> EliminarMetodoPagoAsync(int metodoPagoId)
    {
        var param = new SqlParameter("@MetodoPagoId", metodoPagoId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_metodos_pago_eliminar @MetodoPagoId", param);
    }
}