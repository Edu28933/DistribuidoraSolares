using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;
using System.Linq;

namespace DistribuidoraSolares.Services;

public interface IVentaService
{
    Task<List<Venta>> MostrarVentasAsync(int? sucursalId = null);
    Task<List<Venta>> BuscarVentasAsync(string buscar, int? sucursalId = null);
    Task<VentaCompletaResult> CrearVentaCompletaAsync(VentaCompletaRequest request);
    Task<int> AnularVentaAsync(int ventaId, int usuarioId, string? motivo = null);
    Task<Venta?> ObtenerVentaPorIdAsync(int ventaId);
    Task<VentaCompletaResult> EditarVentaAsync(int ventaId, VentaCompletaRequest request, string observacionEdicion);
}

public class VentaCompletaRequest
{
    public int? ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public int MetodoPagoId { get; set; }
    public string TipoVenta { get; set; } = "NORMAL";
    public decimal DescuentoManual { get; set; }
    public string? Observacion { get; set; }
    public int? ReservaId { get; set; } // ID de reserva si la venta viene de una reserva
    public int? SucursalId { get; set; } // Sucursal donde se realiza la venta
    public List<TVP_VentaDetalle> Detalle { get; set; } = new();
}

public class VentaCompletaResult
{
    public int VentaId { get; set; }
    public decimal TotalNeto { get; set; }
}

public class VentaService : IVentaService
{
    private readonly ApplicationDbContext _context;

    public VentaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Venta>> MostrarVentasAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_ventas_mostrar";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        List<VentaDTO> ventasDTO;
        if (parameters.Any())
        {
            ventasDTO = await _context.Database
                .SqlQueryRaw<VentaDTO>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            ventasDTO = await _context.Database
                .SqlQueryRaw<VentaDTO>(sql)
                .ToListAsync();
        }
        
        // Convertir DTO a Venta con mapeo correcto del cliente
        return ventasDTO.Select(dto => new Venta
        {
            VentaId = dto.VentaId,
            FechaHora = dto.FechaHora,
            ClienteId = dto.ClienteId,
            UsuarioId = dto.UsuarioId,
            MetodoPagoId = dto.MetodoPagoId,
            SucursalId = dto.SucursalId,
            TipoVenta = dto.TipoVenta,
            TotalBruto = dto.TotalBruto,
            DescuentoManual = dto.DescuentoManual,
            TotalNeto = dto.TotalNeto,
            Observacion = dto.Observacion,
            Estado = dto.Estado,
            Cliente = dto.ClienteId.HasValue ? new Cliente
            {
                ClienteId = dto.ClienteId.Value,
                Nombre = dto.ClienteNombre ?? "Sin nombre"
            } : null,
            Usuario = new Usuario
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.UsuarioNombre ?? "Sin nombre"
            },
            MetodoPago = new MetodoPago
            {
                MetodoPagoId = dto.MetodoPagoId,
                Nombre = dto.MetodoPagoNombre ?? "Sin nombre"
            },
            Sucursal = dto.SucursalId.HasValue ? new Sucursal
            {
                SucursalId = dto.SucursalId.Value,
                Nombre = dto.SucursalNombre ?? "Sin nombre"
            } : null
        }).ToList();
    }

    public async Task<List<Venta>> BuscarVentasAsync(string buscar, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Buscar", buscar ?? string.Empty)
        };
        
        var sql = "EXEC usp_ventas_buscar @Buscar";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        var ventasDTO = await _context.Database
            .SqlQueryRaw<VentaDTO>(sql, parameters.ToArray())
            .ToListAsync();
        
        // Convertir DTO a Venta con mapeo correcto del cliente
        return ventasDTO.Select(dto => new Venta
        {
            VentaId = dto.VentaId,
            FechaHora = dto.FechaHora,
            ClienteId = dto.ClienteId,
            UsuarioId = dto.UsuarioId,
            MetodoPagoId = dto.MetodoPagoId,
            SucursalId = dto.SucursalId,
            TipoVenta = dto.TipoVenta,
            TotalBruto = dto.TotalBruto,
            DescuentoManual = dto.DescuentoManual,
            TotalNeto = dto.TotalNeto,
            Observacion = dto.Observacion,
            Estado = dto.Estado,
            Cliente = dto.ClienteId.HasValue ? new Cliente
            {
                ClienteId = dto.ClienteId.Value,
                Nombre = dto.ClienteNombre ?? "Sin nombre"
            } : null,
            Usuario = new Usuario
            {
                UsuarioId = dto.UsuarioId,
                Nombre = dto.UsuarioNombre ?? "Sin nombre"
            },
            MetodoPago = new MetodoPago
            {
                MetodoPagoId = dto.MetodoPagoId,
                Nombre = dto.MetodoPagoNombre ?? "Sin nombre"
            },
            Sucursal = dto.SucursalId.HasValue ? new Sucursal
            {
                SucursalId = dto.SucursalId.Value,
                Nombre = dto.SucursalNombre ?? "Sin nombre"
            } : null
        }).ToList();
    }

    public async Task<VentaCompletaResult> CrearVentaCompletaAsync(VentaCompletaRequest request)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Crear DataTable para el TVP
        var detalleTable = new DataTable();
        detalleTable.Columns.Add("ProductoId", typeof(int));
        detalleTable.Columns.Add("Cantidad", typeof(int));
        detalleTable.Columns.Add("PrecioLista", typeof(decimal));
        detalleTable.Columns.Add("PrecioUnitario", typeof(decimal));

        foreach (var item in request.Detalle)
        {
            detalleTable.Rows.Add(item.ProductoId, item.Cantidad, item.PrecioLista, item.PrecioUnitario);
        }

        using var command = new SqlCommand("usp_venta_completa", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ClienteId", request.ClienteId.HasValue ? (object)request.ClienteId.Value : DBNull.Value));
        command.Parameters.Add(new SqlParameter("@UsuarioId", request.UsuarioId));
        command.Parameters.Add(new SqlParameter("@MetodoPagoId", request.MetodoPagoId));
        command.Parameters.Add(new SqlParameter("@TipoVenta", request.TipoVenta));
        command.Parameters.Add(new SqlParameter("@DescuentoManual", request.DescuentoManual));
        command.Parameters.Add(new SqlParameter("@Observacion", request.Observacion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@SucursalId", request.SucursalId.HasValue ? (object)request.SucursalId.Value : DBNull.Value));
        
        var detalleParam = new SqlParameter("@Detalle", detalleTable)
        {
            SqlDbType = SqlDbType.Structured,
            TypeName = "dbo.TVP_VentaDetalle"
        };
        command.Parameters.Add(detalleParam);

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new VentaCompletaResult
            {
                VentaId = reader.GetInt32(reader.GetOrdinal("VentaId")),
                TotalNeto = reader.GetDecimal(reader.GetOrdinal("TotalNeto"))
            };
        }

        return new VentaCompletaResult();
    }

    public async Task<int> AnularVentaAsync(int ventaId, int usuarioId, string? motivo = null)
    {
        var parameters = new[]
        {
            new SqlParameter("@VentaId", ventaId),
            new SqlParameter("@UsuarioId", usuarioId),
            new SqlParameter("@Motivo", motivo ?? (object)DBNull.Value)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_venta_anular @VentaId, @UsuarioId, @Motivo", parameters);
    }

    public async Task<Venta?> ObtenerVentaPorIdAsync(int ventaId)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Obtener venta con información relacionada
        using var command = new SqlCommand(@"
            SELECT v.VentaId, v.FechaHora, v.ClienteId, v.UsuarioId, v.MetodoPagoId, 
                   v.TipoVenta, v.TotalBruto, v.DescuentoManual, v.TotalNeto, 
                   v.Observacion, v.Estado,
                   c.Nombre AS ClienteNombre,
                   u.Nombre AS UsuarioNombre,
                   mp.Nombre AS MetodoPagoNombre
            FROM tbl_ventas v
            LEFT JOIN tbl_clientes c ON v.ClienteId = c.ClienteId
            INNER JOIN tbl_usuarios u ON v.UsuarioId = u.UsuarioId
            INNER JOIN tbl_metodos_pago mp ON v.MetodoPagoId = mp.MetodoPagoId
            WHERE v.VentaId = @VentaId", connection);

        command.Parameters.Add(new SqlParameter("@VentaId", ventaId));

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return null;
        }

        var venta = new Venta
        {
            VentaId = reader.GetInt32(reader.GetOrdinal("VentaId")),
            FechaHora = reader.GetDateTime(reader.GetOrdinal("FechaHora")),
            ClienteId = reader.IsDBNull(reader.GetOrdinal("ClienteId")) ? null : reader.GetInt32(reader.GetOrdinal("ClienteId")),
            UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
            MetodoPagoId = reader.GetInt32(reader.GetOrdinal("MetodoPagoId")),
            TipoVenta = reader.GetString(reader.GetOrdinal("TipoVenta")),
            TotalBruto = reader.GetDecimal(reader.GetOrdinal("TotalBruto")),
            DescuentoManual = reader.GetDecimal(reader.GetOrdinal("DescuentoManual")),
            TotalNeto = reader.GetDecimal(reader.GetOrdinal("TotalNeto")),
            Observacion = reader.IsDBNull(reader.GetOrdinal("Observacion")) ? null : reader.GetString(reader.GetOrdinal("Observacion")),
            Estado = reader.GetString(reader.GetOrdinal("Estado")),
            Cliente = reader.IsDBNull(reader.GetOrdinal("ClienteId")) ? null : new Models.Cliente
            {
                ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                Nombre = reader.GetString(reader.GetOrdinal("ClienteNombre"))
            },
            Usuario = new Models.Usuario
            {
                UsuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId")),
                Nombre = reader.GetString(reader.GetOrdinal("UsuarioNombre"))
            },
            MetodoPago = new Models.MetodoPago
            {
                MetodoPagoId = reader.GetInt32(reader.GetOrdinal("MetodoPagoId")),
                Nombre = reader.GetString(reader.GetOrdinal("MetodoPagoNombre"))
            }
        };

        reader.Close();

        // Obtener detalles con información de productos
        using var detalleCommand = new SqlCommand(@"
            SELECT vd.VentaDetalleId, vd.VentaId, vd.ProductoId, vd.Cantidad, 
                   vd.PrecioLista, vd.PrecioUnitario, vd.Subtotal,
                   p.Nombre AS ProductoNombre
            FROM tbl_venta_detalle vd
            INNER JOIN tbl_productos p ON vd.ProductoId = p.ProductoId
            WHERE vd.VentaId = @VentaId", connection);

        detalleCommand.Parameters.Add(new SqlParameter("@VentaId", ventaId));

        var detalles = new List<VentaDetalle>();
        using var detalleReader = await detalleCommand.ExecuteReaderAsync();
        while (await detalleReader.ReadAsync())
        {
            detalles.Add(new VentaDetalle
            {
                VentaDetalleId = detalleReader.GetInt32(detalleReader.GetOrdinal("VentaDetalleId")),
                VentaId = detalleReader.GetInt32(detalleReader.GetOrdinal("VentaId")),
                ProductoId = detalleReader.GetInt32(detalleReader.GetOrdinal("ProductoId")),
                Cantidad = detalleReader.GetInt32(detalleReader.GetOrdinal("Cantidad")),
                PrecioLista = detalleReader.GetDecimal(detalleReader.GetOrdinal("PrecioLista")),
                PrecioUnitario = detalleReader.GetDecimal(detalleReader.GetOrdinal("PrecioUnitario")),
                Subtotal = detalleReader.GetDecimal(detalleReader.GetOrdinal("Subtotal")),
                Producto = new Models.Producto
                {
                    ProductoId = detalleReader.GetInt32(detalleReader.GetOrdinal("ProductoId")),
                    Nombre = detalleReader.GetString(detalleReader.GetOrdinal("ProductoNombre"))
                }
            });
        }

        venta.Detalles = detalles;
        return venta;
    }

    public async Task<VentaCompletaResult> EditarVentaAsync(int ventaId, VentaCompletaRequest request, string observacionEdicion)
    {
        // Primero anular la venta original
        await AnularVentaAsync(ventaId, request.UsuarioId, $"Editada: {observacionEdicion}");

        // Crear nueva venta con los datos editados
        var nuevaVenta = await CrearVentaCompletaAsync(request);

        return nuevaVenta;
    }
}