using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IStockService
{
    Task<List<StockActual>> ObtenerStockActualGeneralAsync(int? sucursalId = null);
    Task<StockActual?> ObtenerStockActualPorProductoAsync(int productoId, int? sucursalId = null);
    Task<List<StockPorSucursal>> ObtenerStockPorSucursalAsync(int? sucursalId = null);
}

public class StockService : IStockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockActual>> ObtenerStockActualGeneralAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_stock_actual_general";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        if (parameters.Any())
        {
            return await _context.Database
                .SqlQueryRaw<StockActual>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            return await _context.Database
                .SqlQueryRaw<StockActual>(sql)
                .ToListAsync();
        }
    }

    public async Task<StockActual?> ObtenerStockActualPorProductoAsync(int productoId, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ProductoId", productoId)
        };
        
        var sql = "EXEC usp_stock_actual_por_producto @ProductoId";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        var result = await _context.Database
            .SqlQueryRaw<StockActual>(sql, parameters.ToArray())
            .ToListAsync();

        return result.FirstOrDefault();
    }

    public async Task<List<StockPorSucursal>> ObtenerStockPorSucursalAsync(int? sucursalId = null)
    {
        var sql = "EXEC usp_stock_general_por_sucursal @SucursalId";
        var param = new SqlParameter("@SucursalId", (object?)sucursalId ?? DBNull.Value);
        return await _context.Database
            .SqlQueryRaw<StockPorSucursal>(sql, param)
            .ToListAsync();
    }
}