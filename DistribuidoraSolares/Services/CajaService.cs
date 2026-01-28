using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Services;

public interface ICajaService
{
    Task<int> AbrirCajaAsync(decimal saldoInicial, int usuarioId, int? sucursalId = null);
    Task<CajaCierreResult> CerrarCajaAsync();
    Task<Caja?> ObtenerCajaAbiertaAsync();
    Task<List<Caja>> MostrarCajasAsync(int? sucursalId = null);
}

public class CajaCierreResult
{
    public int CajaId { get; set; }
    public decimal SaldoFinal { get; set; }
}

public class CajaService : ICajaService
{
    private readonly ApplicationDbContext _context;

    public CajaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> AbrirCajaAsync(decimal saldoInicial, int usuarioId, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@SaldoInicial", saldoInicial),
            new SqlParameter("@UsuarioId", usuarioId)
        };
        
        var sql = "EXEC usp_caja_abrir @SaldoInicial, @UsuarioId";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        else
        {
            sql += ", NULL";
        }
        
        var result = await _context.Database
            .SqlQueryRaw<int>(sql, parameters.ToArray())
            .ToListAsync();

        return result.FirstOrDefault();
    }

    public async Task<CajaCierreResult> CerrarCajaAsync()
    {
        var result = await _context.Database
            .SqlQueryRaw<CajaCierreResult>("EXEC usp_caja_cerrar")
            .ToListAsync();

        return result.FirstOrDefault() ?? new CajaCierreResult();
    }

    public async Task<Caja?> ObtenerCajaAbiertaAsync()
    {
        var cajas = await MostrarCajasAsync();
        return cajas.FirstOrDefault(c => c.Estado == "ABIERTA");
    }

    public async Task<List<Caja>> MostrarCajasAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_caja_mostrar";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        if (parameters.Any())
        {
            return await _context.Database
                .SqlQueryRaw<Caja>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            return await _context.Database
                .SqlQueryRaw<Caja>(sql)
                .ToListAsync();
        }
    }
}