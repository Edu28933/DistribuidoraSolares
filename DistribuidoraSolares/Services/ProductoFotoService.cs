using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IProductoFotoService
{
    Task<List<ProductoFoto>> ObtenerFotosPorProductoAsync(int productoId);
    Task<int> SubirFotoAsync(int productoId, string urlFoto, bool esPrincipal, int usuarioId);
    Task<int> EliminarFotoAsync(int fotoId);
    Task<int> MarcarComoPrincipalAsync(int fotoId, int productoId);
}

public class ProductoFotoService : IProductoFotoService
{
    private readonly ApplicationDbContext _context;

    public ProductoFotoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoFoto>> ObtenerFotosPorProductoAsync(int productoId)
    {
        var param = new SqlParameter("@ProductoId", productoId);
        return await _context.Database
            .SqlQueryRaw<ProductoFoto>("EXEC usp_producto_fotos_por_producto @ProductoId", param)
            .ToListAsync();
    }

    public async Task<int> SubirFotoAsync(int productoId, string urlFoto, bool esPrincipal, int usuarioId)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Si hay otra foto marcada como principal para este producto, desmarcarla primero
        if (esPrincipal)
        {
            var fotosExistentes = await ObtenerFotosPorProductoAsync(productoId);
            var fotoPrincipalExistente = fotosExistentes.FirstOrDefault(f => f.EsPrincipal);
            if (fotoPrincipalExistente != null)
            {
                using var updateCommand = new SqlCommand(
                    "UPDATE tbl_producto_fotos SET EsPrincipal = 0 WHERE FotoId = @FotoId", 
                    connection);
                updateCommand.Parameters.Add(new SqlParameter("@FotoId", fotoPrincipalExistente.FotoId));
                await updateCommand.ExecuteNonQueryAsync();
            }
        }

        // Usar el stored procedure correcto
        using var command = new SqlCommand("usp_producto_fotos_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ProductoId", productoId));
        command.Parameters.Add(new SqlParameter("@UrlFoto", urlFoto));
        command.Parameters.Add(new SqlParameter("@EsPrincipal", esPrincipal ? 1 : 0));
        command.Parameters.Add(new SqlParameter("@Estado", "ACTIVO"));
        command.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));

        var result = await command.ExecuteScalarAsync();
        
        // SCOPE_IDENTITY() puede devolver decimal, convertir a int
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EliminarFotoAsync(int fotoId)
    {
        var param = new SqlParameter("@FotoId", fotoId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_producto_fotos_eliminar @FotoId", param);
    }

    public async Task<int> MarcarComoPrincipalAsync(int fotoId, int productoId)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Primero desmarcar todas las fotos principales de este producto
        using var updateCommand = new SqlCommand(
            "UPDATE tbl_producto_fotos SET EsPrincipal = 0 WHERE ProductoId = @ProductoId AND FotoId != @FotoId", 
            connection);
        updateCommand.Parameters.Add(new SqlParameter("@ProductoId", productoId));
        updateCommand.Parameters.Add(new SqlParameter("@FotoId", fotoId));
        await updateCommand.ExecuteNonQueryAsync();

        // Luego marcar la foto seleccionada como principal usando el stored procedure de editar
        using var command = new SqlCommand("usp_producto_fotos_editar", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Obtener los datos actuales de la foto para editarla
        var fotos = await ObtenerFotosPorProductoAsync(productoId);
        var foto = fotos.FirstOrDefault(f => f.FotoId == fotoId);
        
        if (foto == null)
            return 0;

        command.Parameters.Add(new SqlParameter("@FotoId", fotoId));
        command.Parameters.Add(new SqlParameter("@ProductoId", productoId));
        command.Parameters.Add(new SqlParameter("@UrlFoto", foto.UrlFoto));
        command.Parameters.Add(new SqlParameter("@EsPrincipal", 1)); // Marcar como principal
        command.Parameters.Add(new SqlParameter("@Estado", foto.Estado));
        command.Parameters.Add(new SqlParameter("@UsuarioId", foto.UsuarioId));

        var result = await command.ExecuteScalarAsync();
        
        return result != null ? Convert.ToInt32(result) : 0;
    }
}
