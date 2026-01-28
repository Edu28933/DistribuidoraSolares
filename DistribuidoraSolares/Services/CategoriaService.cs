using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface ICategoriaService
{
    Task<List<Categoria>> MostrarCategoriasAsync();
    Task<List<Categoria>> BuscarCategoriasAsync(string buscar);
    Task<int> CrearCategoriaAsync(Categoria categoria);
    Task<int> EditarCategoriaAsync(Categoria categoria);
    Task<int> EliminarCategoriaAsync(int categoriaId);
}

public class CategoriaService : ICategoriaService
{
    private readonly ApplicationDbContext _context;

    public CategoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Categoria>> MostrarCategoriasAsync()
    {
        return await _context.Database
            .SqlQueryRaw<Categoria>("EXEC usp_categorias_mostrar")
            .ToListAsync();
    }

    public async Task<List<Categoria>> BuscarCategoriasAsync(string buscar)
    {
        var param = new SqlParameter("@Buscar", buscar ?? string.Empty);
        return await _context.Database
            .SqlQueryRaw<Categoria>("EXEC usp_categorias_buscar @Buscar", param)
            .ToListAsync();
    }

    public async Task<int> CrearCategoriaAsync(Categoria categoria)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_categorias_crear", connection)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@Nombre", categoria.Nombre));
        command.Parameters.Add(new SqlParameter("@Estado", categoria.Estado));

        var result = await command.ExecuteScalarAsync();
        
        // SCOPE_IDENTITY() puede devolver decimal, convertir a int
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }

    public async Task<int> EditarCategoriaAsync(Categoria categoria)
    {
        var parameters = new[]
        {
            new SqlParameter("@CategoriaId", categoria.CategoriaId),
            new SqlParameter("@Nombre", categoria.Nombre),
            new SqlParameter("@Estado", categoria.Estado)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_categorias_editar @CategoriaId, @Nombre, @Estado", parameters);
    }

    public async Task<int> EliminarCategoriaAsync(int categoriaId)
    {
        var param = new SqlParameter("@CategoriaId", categoriaId);
        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_categorias_eliminar @CategoriaId", param);
    }
}