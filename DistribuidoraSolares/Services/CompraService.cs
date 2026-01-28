using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface ICompraService
{
    Task<List<Compra>> MostrarComprasAsync(int? sucursalId = null);
    Task<List<Compra>> BuscarComprasAsync(string buscar, int? sucursalId = null);
    Task<CompraCompletaResult> CrearCompraCompletaAsync(CompraCompletaRequest request);
}

public class CompraCompletaRequest
{
    public int ProveedorId { get; set; }
    public int UsuarioId { get; set; }
    public string? Descripcion { get; set; }
    public int? SucursalId { get; set; } // Sucursal donde se recibe la compra
    public List<TVP_CompraDetalle> Detalle { get; set; } = new();
}

public class CompraCompletaResult
{
    public int CompraId { get; set; }
    public decimal Total { get; set; }
}

public class CompraService : ICompraService
{
    private readonly ApplicationDbContext _context;

    public CompraService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Compra>> MostrarComprasAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_compras_mostrar";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        if (parameters.Any())
        {
            return await _context.Database
                .SqlQueryRaw<Compra>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            return await _context.Database
                .SqlQueryRaw<Compra>(sql)
                .ToListAsync();
        }
    }

    public async Task<List<Compra>> BuscarComprasAsync(string buscar, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Buscar", buscar ?? string.Empty)
        };
        
        var sql = "EXEC usp_compras_buscar @Buscar";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        return await _context.Database
            .SqlQueryRaw<Compra>(sql, parameters.ToArray())
            .ToListAsync();
    }

    public async Task<CompraCompletaResult> CrearCompraCompletaAsync(CompraCompletaRequest request)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Crear DataTable para el TVP
        var detalleTable = new DataTable();
        detalleTable.Columns.Add("ProductoId", typeof(int));
        detalleTable.Columns.Add("Cantidad", typeof(int));
        detalleTable.Columns.Add("CostoUnitario", typeof(decimal));

        foreach (var item in request.Detalle)
        {
            detalleTable.Rows.Add(item.ProductoId, item.Cantidad, item.CostoUnitario);
        }

        using var command = new SqlCommand("usp_compra_completa", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ProveedorId", request.ProveedorId));
        command.Parameters.Add(new SqlParameter("@UsuarioId", request.UsuarioId));
        command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@SucursalId", request.SucursalId.HasValue ? (object)request.SucursalId.Value : DBNull.Value));
        
        var detalleParam = new SqlParameter("@Detalle", detalleTable)
        {
            SqlDbType = SqlDbType.Structured,
            TypeName = "dbo.TVP_CompraDetalle"
        };
        command.Parameters.Add(detalleParam);

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new CompraCompletaResult
            {
                CompraId = reader.GetInt32(reader.GetOrdinal("CompraId")),
                Total = reader.GetDecimal(reader.GetOrdinal("Total"))
            };
        }

        return new CompraCompletaResult();
    }
}
