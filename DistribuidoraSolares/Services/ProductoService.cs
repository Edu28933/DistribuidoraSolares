using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IProductoService
{
    Task<List<Producto>> MostrarProductosAsync();
    Task<List<Producto>> BuscarProductosAsync(string buscar);
    Task<int> CrearProductoAsync(Producto producto);
    Task<int> EditarProductoAsync(Producto producto);
    Task<int> EliminarProductoAsync(int productoId);
}

public class ProductoService : IProductoService
{
    private readonly ApplicationDbContext _context;

    public ProductoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> MostrarProductosAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Producto>("EXEC usp_productos_mostrar")
            .ToListAsync();
    }

    public async Task<List<Producto>> BuscarProductosAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Producto>("EXEC usp_productos_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearProductoAsync(Producto producto)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_productos_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@CategoriaId", producto.CategoriaId));
        command.Parameters.Add(new SqlParameter("@Codigo", producto.Codigo));
        command.Parameters.Add(new SqlParameter("@Nombre", producto.Nombre));
        command.Parameters.Add(new SqlParameter("@PrecioVenta", producto.PrecioVenta));
        command.Parameters.Add(new SqlParameter("@CostoReferencia", producto.CostoReferencia));
        command.Parameters.Add(new SqlParameter("@StockMinimo", producto.StockMinimo));
        command.Parameters.Add(new SqlParameter("@Estado", producto.Estado));

        var result = await command.ExecuteScalarAsync();
        
        // SCOPE_IDENTITY() puede devolver decimal, convertir a int
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarProductoAsync(Producto producto)
    {
        var parameters = new[]
        {
            new SqlParameter("@ProductoId", producto.ProductoId),
            new SqlParameter("@CategoriaId", producto.CategoriaId),
            new SqlParameter("@Codigo", producto.Codigo),
            new SqlParameter("@Nombre", producto.Nombre),
            new SqlParameter("@PrecioVenta", producto.PrecioVenta),
            new SqlParameter("@CostoReferencia", producto.CostoReferencia),
            new SqlParameter("@StockMinimo", producto.StockMinimo),
            new SqlParameter("@Estado", producto.Estado)
        };

        var result = await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_productos_editar @ProductoId, @CategoriaId, @Codigo, @Nombre, @PrecioVenta, @CostoReferencia, @StockMinimo, @Estado", parameters);

        return result;
    }

    public async Task<int> EliminarProductoAsync(int productoId)
    {
        var param = new SqlParameter("@ProductoId", productoId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_productos_eliminar @ProductoId", param);
    }
}