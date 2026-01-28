using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IUsuarioEmailService
{
    Task<UsuarioEmail?> ObtenerEmailPorUsuarioAsync(int usuarioId);
    Task<int> CrearOActualizarEmailAsync(int usuarioId, string email);
}

public class UsuarioEmailService : IUsuarioEmailService
{
    private readonly ApplicationDbContext _context;

    public UsuarioEmailService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UsuarioEmail?> ObtenerEmailPorUsuarioAsync(int usuarioId)
    {
        var param = new SqlParameter("@UsuarioId", usuarioId);
        var emails = await _context.Database
            .SqlQueryRaw<UsuarioEmail>("EXEC usp_usuario_emails_obtener @UsuarioId", param)
            .ToListAsync();
        
        return emails.FirstOrDefault();
    }

    public async Task<int> CrearOActualizarEmailAsync(int usuarioId, string email)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_usuario_emails_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));
        command.Parameters.Add(new SqlParameter("@Email", email));

        var result = await command.ExecuteScalarAsync();
        
        if (result != null)
        {
            return Convert.ToInt32(result);
        }
        
        return 0;
    }
}
