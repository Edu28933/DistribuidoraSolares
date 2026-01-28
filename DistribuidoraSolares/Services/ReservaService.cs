using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IReservaService
{
    Task<List<Reserva>> MostrarReservasAsync(int? sucursalId = null);
    Task<List<Reserva>> BuscarReservasAsync(string buscar, int? sucursalId = null);
    Task<ReservaCompletaResult> CrearReservaCompletaAsync(ReservaCompletaRequest request);
    Task<int> CancelarReservaAsync(int reservaId, int usuarioId, string? motivo = null);
    Task<int> ConvertirReservaAVentaAsync(int reservaId, int usuarioId);
    Task<List<ReservaConDetalles>> ObtenerReservasActivasPorClienteAsync(int clienteId);
    Task<int> MarcarReservaComoCompletadaAsync(int reservaId);
    Task<int> MarcarDetallesReservaComoOcupadosAsync(int reservaId, List<VentaDetalleReserva>? detallesVenta = null);
    Task<int> LiberarProductosReservaParaVentaAsync(int reservaId, int ventaId, int usuarioId, List<VentaDetalleReserva> detallesVenta);
    Task<int> ActualizarMovimientosLiberacionConVentaIdAsync(int reservaId, int ventaId);
}

public class VentaDetalleReserva
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}

public class ReservaConDetalles
{
    public int ReservaId { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public List<ReservaDetalleInfo> Detalles { get; set; } = new();
}

public class ReservaDetalleInfo
{
    public int ReservaDetalleId { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal PrecioVenta { get; set; }
    public int CantidadReservada { get; set; }
    public int StockDisponible { get; set; }
}

public class ReservaCompletaRequest
{
    public int ClienteId { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string? Observacion { get; set; }
    public int? SucursalId { get; set; } // Sucursal donde se crea la reserva
    public List<TVP_ReservaDetalle> Detalle { get; set; } = new();
}

public class ReservaCompletaResult
{
    public int ReservaId { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class ReservaService : IReservaService
{
    private readonly ApplicationDbContext _context;

    public ReservaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Reserva>> MostrarReservasAsync(int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>();
        var sql = "EXEC usp_reservas_mostrar";
        
        if (sucursalId.HasValue)
        {
            sql += " @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        if (parameters.Any())
        {
            return await _context.Database
                .SqlQueryRaw<Reserva>(sql, parameters.ToArray())
                .ToListAsync();
        }
        else
        {
            return await _context.Database
                .SqlQueryRaw<Reserva>(sql)
                .ToListAsync();
        }
    }

    public async Task<List<Reserva>> BuscarReservasAsync(string buscar, int? sucursalId = null)
    {
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@Buscar", buscar ?? string.Empty)
        };
        
        var sql = "EXEC usp_reservas_buscar @Buscar";
        
        if (sucursalId.HasValue)
        {
            sql += ", @SucursalId";
            parameters.Add(new SqlParameter("@SucursalId", sucursalId.Value));
        }
        
        return await _context.Database
            .SqlQueryRaw<Reserva>(sql, parameters.ToArray())
            .ToListAsync();
    }

    public async Task<ReservaCompletaResult> CrearReservaCompletaAsync(ReservaCompletaRequest request)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Crear DataTable para el TVP
        var detalleTable = new DataTable();
        detalleTable.Columns.Add("ProductoId", typeof(int));
        detalleTable.Columns.Add("CantidadReservada", typeof(int));

        foreach (var item in request.Detalle)
        {
            detalleTable.Rows.Add(item.ProductoId, item.CantidadReservada);
        }

        using var command = new SqlCommand("usp_reserva_crear_completa", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ClienteId", request.ClienteId));
        command.Parameters.Add(new SqlParameter("@UsuarioId", request.UsuarioId));
        command.Parameters.Add(new SqlParameter("@FechaVencimiento", request.FechaVencimiento));
        command.Parameters.Add(new SqlParameter("@Observacion", request.Observacion ?? (object)DBNull.Value));
        command.Parameters.Add(new SqlParameter("@SucursalId", request.SucursalId.HasValue ? (object)request.SucursalId.Value : DBNull.Value));
        
        var detalleParam = new SqlParameter("@Detalle", detalleTable)
        {
            SqlDbType = SqlDbType.Structured,
            TypeName = "dbo.TVP_ReservaDetalle"
        };
        command.Parameters.Add(detalleParam);

        using var reader = await command.ExecuteReaderAsync();
        
        if (await reader.ReadAsync())
        {
            return new ReservaCompletaResult
            {
                ReservaId = reader.GetInt32(reader.GetOrdinal("ReservaId")),
                Estado = reader.GetString(reader.GetOrdinal("Estado"))
            };
        }

        return new ReservaCompletaResult();
    }

    public async Task<int> CancelarReservaAsync(int reservaId, int usuarioId, string? motivo = null)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservaId", reservaId),
            new SqlParameter("@UsuarioId", usuarioId),
            new SqlParameter("@Motivo", motivo ?? (object)DBNull.Value)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_reserva_cancelar @ReservaId, @UsuarioId, @Motivo", parameters);
    }

    public async Task<int> ConvertirReservaAVentaAsync(int reservaId, int usuarioId)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservaId", reservaId),
            new SqlParameter("@UsuarioId", usuarioId)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("EXEC usp_reserva_convertir_venta @ReservaId, @UsuarioId", parameters);
    }

    public async Task<List<ReservaConDetalles>> ObtenerReservasActivasPorClienteAsync(int clienteId)
    {
        var connectionString = _context.Database.GetConnectionString();
        var reservas = new List<ReservaConDetalles>();

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Obtener reservas activas del cliente
        using var command = new SqlCommand(@"
            SELECT r.ReservaId, r.ClienteId, c.Nombre AS ClienteNombre, 
                   r.FechaHora, r.FechaVencimiento, r.Estado, r.Observacion
            FROM tbl_reservas r
            INNER JOIN tbl_clientes c ON r.ClienteId = c.ClienteId
            WHERE r.ClienteId = @ClienteId 
              AND r.Estado = 'ACTIVA'
              AND r.FechaVencimiento >= GETDATE()
            ORDER BY r.FechaHora DESC", connection);

        command.Parameters.Add(new SqlParameter("@ClienteId", clienteId));

        using var reader = await command.ExecuteReaderAsync();
        var reservaIds = new List<int>();

        while (await reader.ReadAsync())
        {
            var reserva = new ReservaConDetalles
            {
                ReservaId = reader.GetInt32(reader.GetOrdinal("ReservaId")),
                ClienteId = reader.GetInt32(reader.GetOrdinal("ClienteId")),
                ClienteNombre = reader.GetString(reader.GetOrdinal("ClienteNombre")),
                FechaHora = reader.GetDateTime(reader.GetOrdinal("FechaHora")),
                FechaVencimiento = reader.GetDateTime(reader.GetOrdinal("FechaVencimiento")),
                Estado = reader.GetString(reader.GetOrdinal("Estado")),
                Observacion = reader.IsDBNull(reader.GetOrdinal("Observacion")) ? null : reader.GetString(reader.GetOrdinal("Observacion"))
            };
            reservas.Add(reserva);
            reservaIds.Add(reserva.ReservaId);
        }

        reader.Close();

        // Obtener detalles de las reservas
        if (reservaIds.Any())
        {
            var idsString = string.Join(",", reservaIds);
            // Usar el mismo cálculo que el stored procedure usp_stock_actual_general
            // Suma TODOS los movimientos donde Estado='ACTIVO' (incluye COMPRA, VENTA, PERDIDA, RESERVA, LIBERACION_RESERVA, AJUSTE)
            using var detalleCommand = new SqlCommand($@"
                SELECT rd.ReservaDetalleId, rd.ReservaId, rd.ProductoId, 
                       p.Nombre AS ProductoNombre, p.PrecioVenta, rd.CantidadReservada,
                       ISNULL(
                           (SELECT ISNULL(SUM(CASE WHEN Estado = 'ACTIVO' THEN Cantidad ELSE 0 END), 0)
                            FROM tbl_inventario_movimientos
                            WHERE ProductoId = p.ProductoId), 0
                       ) AS StockDisponible
                FROM tbl_reserva_detalle rd
                INNER JOIN tbl_productos p ON rd.ProductoId = p.ProductoId
                WHERE rd.ReservaId IN ({idsString})
                  AND rd.Estado IN ('ACTIVO', 'OCUPADO')
                ORDER BY rd.ReservaId, rd.ProductoId", connection);

            using var detalleReader = await detalleCommand.ExecuteReaderAsync();
            while (await detalleReader.ReadAsync())
            {
                var reservaId = detalleReader.GetInt32(detalleReader.GetOrdinal("ReservaId"));
                var reserva = reservas.FirstOrDefault(r => r.ReservaId == reservaId);
                if (reserva != null)
                {
                    reserva.Detalles.Add(new ReservaDetalleInfo
                    {
                        ReservaDetalleId = detalleReader.GetInt32(detalleReader.GetOrdinal("ReservaDetalleId")),
                        ProductoId = detalleReader.GetInt32(detalleReader.GetOrdinal("ProductoId")),
                        ProductoNombre = detalleReader.GetString(detalleReader.GetOrdinal("ProductoNombre")),
                        PrecioVenta = detalleReader.GetDecimal(detalleReader.GetOrdinal("PrecioVenta")),
                        CantidadReservada = detalleReader.GetInt32(detalleReader.GetOrdinal("CantidadReservada")),
                        StockDisponible = detalleReader.GetInt32(detalleReader.GetOrdinal("StockDisponible"))
                    });
                }
            }
        }

        return reservas;
    }

    public async Task<int> MarcarReservaComoCompletadaAsync(int reservaId)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservaId", reservaId)
        };

        return await _context.Database
            .ExecuteSqlRawAsync("UPDATE tbl_reservas SET Estado = 'CONVERTIDA' WHERE ReservaId = @ReservaId", parameters);
    }

    public async Task<int> MarcarDetallesReservaComoOcupadosAsync(int reservaId, List<VentaDetalleReserva>? detallesVenta = null)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        if (detallesVenta != null && detallesVenta.Any())
        {
            // Marcar solo los productos que están en la venta
            // Agrupar por ProductoId para manejar múltiples detalles del mismo producto
            var productosVenta = detallesVenta
                .GroupBy(d => d.ProductoId)
                .Select(g => new { ProductoId = g.Key, CantidadTotal = g.Sum(d => d.Cantidad) })
                .ToList();

            int detallesMarcados = 0;
            foreach (var productoVenta in productosVenta)
            {
                // Marcar los detalles de reserva que corresponden a este producto
                // Usar un CTE para marcar solo la cantidad vendida
                using var command = new SqlCommand(@"
                    WITH DetallesParaMarcar AS (
                        SELECT TOP(@Cantidad) ReservaDetalleId
                        FROM tbl_reserva_detalle
                        WHERE ReservaId = @ReservaId 
                          AND ProductoId = @ProductoId 
                          AND Estado = 'ACTIVO'
                        ORDER BY ReservaDetalleId
                    )
                    UPDATE rd
                    SET rd.Estado = 'OCUPADO'
                    FROM tbl_reserva_detalle rd
                    INNER JOIN DetallesParaMarcar dpm ON rd.ReservaDetalleId = dpm.ReservaDetalleId", connection);

                command.Parameters.Add(new SqlParameter("@ReservaId", reservaId));
                command.Parameters.Add(new SqlParameter("@ProductoId", productoVenta.ProductoId));
                command.Parameters.Add(new SqlParameter("@Cantidad", productoVenta.CantidadTotal));

                detallesMarcados += await command.ExecuteNonQueryAsync();
            }

            return detallesMarcados;
        }
        else
        {
            // Si no se especifican detalles, marcar todos (comportamiento anterior)
            var parameters = new[]
            {
                new SqlParameter("@ReservaId", reservaId)
            };

            return await _context.Database
                .ExecuteSqlRawAsync("UPDATE tbl_reserva_detalle SET Estado = 'OCUPADO' WHERE ReservaId = @ReservaId AND Estado = 'ACTIVO'", parameters);
        }
    }

    public async Task<int> LiberarProductosReservaParaVentaAsync(int reservaId, int ventaId, int usuarioId, List<VentaDetalleReserva> detallesVenta)
    {
        var connectionString = _context.Database.GetConnectionString();
        
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Agrupar productos de la venta por ProductoId para sumar cantidades
        var productosVenta = detallesVenta
            .GroupBy(d => d.ProductoId)
            .Select(g => new { ProductoId = g.Key, CantidadTotal = g.Sum(d => d.Cantidad) })
            .ToList();

        // Obtener los detalles de reserva que están siendo aplicados, agrupados por producto
        using var command = new SqlCommand(@"
            SELECT rd.ProductoId, SUM(rd.CantidadReservada) AS CantidadTotalReservada
            FROM tbl_reserva_detalle rd
            WHERE rd.ReservaId = @ReservaId 
              AND rd.Estado = 'ACTIVO'
            GROUP BY rd.ProductoId", connection);

        command.Parameters.Add(new SqlParameter("@ReservaId", reservaId));

        var detallesReserva = new Dictionary<int, int>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            detallesReserva[reader.GetInt32(reader.GetOrdinal("ProductoId"))] = 
                reader.GetInt32(reader.GetOrdinal("CantidadTotalReservada"));
        }
        reader.Close();

        // Obtener SucursalId de la reserva
        int? sucursalIdReserva = null;
        using var sucursalCommand = new SqlCommand(@"
            SELECT SucursalId 
            FROM tbl_reservas 
            WHERE ReservaId = @ReservaId", connection);
        sucursalCommand.Parameters.Add(new SqlParameter("@ReservaId", reservaId));
        var sucursalResult = await sucursalCommand.ExecuteScalarAsync();
        if (sucursalResult != null && sucursalResult != DBNull.Value)
        {
            sucursalIdReserva = Convert.ToInt32(sucursalResult);
        }

        // Crear movimientos LIBERACION_RESERVA solo para los productos que están en la venta
        int movimientosCreados = 0;
        foreach (var productoVenta in productosVenta)
        {
            // Buscar si hay reserva para este producto
            if (detallesReserva.TryGetValue(productoVenta.ProductoId, out int cantidadReservada))
            {
                // Calcular la cantidad a liberar (mínimo entre lo reservado y lo vendido)
                int cantidadALiberar = Math.Min(cantidadReservada, productoVenta.CantidadTotal);
                
                if (cantidadALiberar > 0)
                {
                    using var insertCommand = new SqlCommand(@"
                        INSERT INTO tbl_inventario_movimientos
                        (ProductoId, FechaHora, Tipo, Cantidad, CostoUnitario, VentaId, CompraId, ReservaId, Descripcion, UsuarioId, Estado, SucursalId)
                        VALUES
                        (@ProductoId, GETDATE(), 'LIBERACION_RESERVA', @Cantidad, NULL, @VentaId, NULL, @ReservaId, 
                         CASE WHEN @VentaId > 0 THEN 'Liberación de reserva por venta #' + CAST(@VentaId AS VARCHAR(10)) ELSE 'Liberación de reserva para venta' END, 
                         @UsuarioId, 'ACTIVO', @SucursalId)", connection);

                    insertCommand.Parameters.Add(new SqlParameter("@ProductoId", productoVenta.ProductoId));
                    insertCommand.Parameters.Add(new SqlParameter("@Cantidad", cantidadALiberar));
                    insertCommand.Parameters.Add(new SqlParameter("@VentaId", ventaId > 0 ? (object)ventaId : DBNull.Value));
                    insertCommand.Parameters.Add(new SqlParameter("@ReservaId", reservaId));
                    insertCommand.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));
                    insertCommand.Parameters.Add(new SqlParameter("@SucursalId", sucursalIdReserva.HasValue ? (object)sucursalIdReserva.Value : DBNull.Value));

                    await insertCommand.ExecuteNonQueryAsync();
                    movimientosCreados++;
                }
            }
        }

        return movimientosCreados;
    }

    public async Task<int> ActualizarMovimientosLiberacionConVentaIdAsync(int reservaId, int ventaId)
    {
        var parameters = new[]
        {
            new SqlParameter("@ReservaId", reservaId),
            new SqlParameter("@VentaId", ventaId)
        };

        // Actualizar los movimientos LIBERACION_RESERVA que tienen ReservaId pero VentaId NULL
        // con el VentaId correcto
        return await _context.Database
            .ExecuteSqlRawAsync(@"
                UPDATE tbl_inventario_movimientos 
                SET VentaId = @VentaId,
                    Descripcion = 'Liberación de reserva por venta #' + CAST(@VentaId AS VARCHAR(10))
                WHERE ReservaId = @ReservaId 
                  AND Tipo = 'LIBERACION_RESERVA'
                  AND (VentaId IS NULL OR VentaId = 0)
                  AND Estado = 'ACTIVO'", parameters);
    }
}
