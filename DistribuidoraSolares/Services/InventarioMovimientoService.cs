using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Linq;

namespace DistribuidoraSolares.Services;

public interface IInventarioMovimientoService
{
    Task<List<InventarioMovimiento>> MostrarMovimientosAsync(int? sucursalId = null);
    Task<List<InventarioMovimiento>> BuscarMovimientosAsync(string buscar, int? sucursalId = null);
    Task<List<InventarioMovimiento>> ObtenerMovimientosPorProductoAsync(int productoId, int? sucursalId = null);
}

public class InventarioMovimientoService : IInventarioMovimientoService
{
    private readonly ApplicationDbContext _context;

    public InventarioMovimientoService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventarioMovimiento>> MostrarMovimientosAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_inventario_movimientos_mostrar";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        List<InventarioMovimientoDTO> movimientosDTO;
        if (parameters.Any())
        {
            movimientosDTO = await _context.Database
                .SqlQueryRaw<InventarioMovimientoDTO>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            movimientosDTO = await _context.Database
                .SqlQueryRaw<InventarioMovimientoDTO>(sql)
                .ToListAsync();
        }
        
        // Convertir DTO a InventarioMovimiento con mapeo correcto
        return movimientosDTO.Select(dto => new InventarioMovimiento
        {
            InvMovId = dto.InvMovId,
            ProductoId = dto.ProductoId,
            SucursalId = dto.SucursalId,
            FechaHora = dto.FechaHora,
            Tipo = dto.Tipo,
            Cantidad = dto.Cantidad,
            CostoUnitario = dto.CostoUnitario,
            VentaId = dto.VentaId,
            CompraId = dto.CompraId,
            ReservaId = dto.ReservaId,
            Descripcion = dto.Descripcion,
            UsuarioId = dto.UsuarioId,
            Estado = dto.Estado,
            Producto = new Producto
            {
                ProductoId = dto.ProductoId,
                Nombre = dto.ProductoNombre ?? "Sin nombre"
            },
            Usuario = new Usuario
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.UsuarioNombre ?? "Sin nombre"
            },
            Sucursal = dto.SucursalId.HasValue ? new Sucursal
            {
                SucursalId = dto.SucursalId.Value,
                Nombre = dto.SucursalNombre ?? "Sin nombre"
            } : null
        }).ToList();
    }

    public async Task<List<InventarioMovimiento>> BuscarMovimientosAsync(string buscar, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Buscar", buscar ?? string.Empty)
        };
        
        var sql = "EXEC usp_inventario_movimientos_buscar @Buscar";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        var movimientosDTO = await _context.Database
            .SqlQueryRaw<InventarioMovimientoDTO>(sql, parameters.ToArray())
            .ToListAsync();
        
        // Convertir DTO a InventarioMovimiento con mapeo correcto
        return movimientosDTO.Select(dto => new InventarioMovimiento
        {
            InvMovId = dto.InvMovId,
            ProductoId = dto.ProductoId,
            SucursalId = dto.SucursalId,
            FechaHora = dto.FechaHora,
            Tipo = dto.Tipo,
            Cantidad = dto.Cantidad,
            CostoUnitario = dto.CostoUnitario,
            VentaId = dto.VentaId,
            CompraId = dto.CompraId,
            ReservaId = dto.ReservaId,
            Descripcion = dto.Descripcion,
            UsuarioId = dto.UsuarioId,
            Estado = dto.Estado,
            Producto = new Producto
            {
                ProductoId = dto.ProductoId,
                Nombre = dto.ProductoNombre ?? "Sin nombre"
            },
            Usuario = new Usuario
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.UsuarioNombre ?? "Sin nombre"
            },
            Sucursal = dto.SucursalId.HasValue ? new Sucursal
            {
                SucursalId = dto.SucursalId.Value,
                Nombre = dto.SucursalNombre ?? "Sin nombre"
            } : null
        }).ToList();
    }

    public async Task<List<InventarioMovimiento>> ObtenerMovimientosPorProductoAsync(int productoId, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ProductoId", productoId)
        };
        
        var sql = "EXEC usp_inventario_movimientos_por_producto @ProductoId";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        var movimientosDTO = await _context.Database
            .SqlQueryRaw<InventarioMovimientoDTO>(sql, parameters.ToArray())
            .ToListAsync();
        
        // Convertir DTO a InventarioMovimiento con mapeo correcto
        return movimientosDTO.Select(dto => new InventarioMovimiento
        {
            InvMovId = dto.InvMovId,
            ProductoId = dto.ProductoId,
            SucursalId = dto.SucursalId,
            FechaHora = dto.FechaHora,
            Tipo = dto.Tipo,
            Cantidad = dto.Cantidad,
            CostoUnitario = dto.CostoUnitario,
            VentaId = dto.VentaId,
            CompraId = dto.CompraId,
            ReservaId = dto.ReservaId,
            Descripcion = dto.Descripcion,
            UsuarioId = dto.UsuarioId,
            Estado = dto.Estado,
            Producto = new Producto
            {
                ProductoId = dto.ProductoId,
                Nombre = dto.ProductoNombre ?? "Sin nombre"
            },
            Usuario = new Usuario
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.UsuarioNombre ?? "Sin nombre"
            },
            Sucursal = dto.SucursalId.HasValue ? new Sucursal
            {
                SucursalId = dto.SucursalId.Value,
                Nombre = dto.SucursalNombre ?? "Sin nombre"
            } : null
        }).ToList();
    }
}
