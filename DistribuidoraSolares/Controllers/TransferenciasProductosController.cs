using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using System.Data;

namespace DistribuidoraSolares.Controllers;

public class TransferenciasProductosController : Controller
{
    private readonly IStockService _stockService;
    private readonly ISucursalService _sucursalService;
    private readonly IProductoService _productoService;
    private readonly IInventarioMovimientoService _inventarioMovimientoService;
    private readonly ApplicationDbContext _context;

    public TransferenciasProductosController(
        IStockService stockService,
        ISucursalService sucursalService,
        IProductoService productoService,
        IInventarioMovimientoService inventarioMovimientoService,
        ApplicationDbContext context)
    {
        _stockService = stockService;
        _sucursalService = sucursalService;
        _productoService = productoService;
        _inventarioMovimientoService = inventarioMovimientoService;
        _context = context;
    }

    private async Task CrearMovimientoTransferencia(int productoId, int sucursalId, int cantidad, string descripcion, int usuarioId)
    {
        var connectionString = _context.Database.GetConnectionString();
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("usp_inventario_movimientos_crear", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@ProductoId", productoId));
        command.Parameters.Add(new SqlParameter("@Tipo", "AJUSTE"));
        command.Parameters.Add(new SqlParameter("@Cantidad", cantidad));
        command.Parameters.Add(new SqlParameter("@CostoUnitario", DBNull.Value));
        command.Parameters.Add(new SqlParameter("@VentaId", DBNull.Value));
        command.Parameters.Add(new SqlParameter("@CompraId", DBNull.Value));
        command.Parameters.Add(new SqlParameter("@ReservaId", DBNull.Value));
        command.Parameters.Add(new SqlParameter("@Descripcion", descripcion));
        command.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));
        command.Parameters.Add(new SqlParameter("@Estado", "ACTIVO"));
        command.Parameters.Add(new SqlParameter("@SucursalId", sucursalId));

        await command.ExecuteNonQueryAsync();
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("Super Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> Index()
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "Solo el SuperAdmin puede transferir productos entre sucursales";
            return RedirectToAction("Index", "Dashboard");
        }

        try
        {
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            var sucursalesActivas = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            var productos = await _productoService.MostrarProductosAsync();
            var productosDisponibles = productos.Where(p => p.Estado == "DISPONIBLE").ToList();

            ViewBag.Sucursales = sucursalesActivas;
            ViewBag.Productos = productosDisponibles;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos: {ex.Message}";
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transferir(int productoId, int sucursalOrigenId, int sucursalDestinoId, int cantidad, string? observacion)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "Solo el SuperAdmin puede transferir productos entre sucursales";
            return RedirectToAction("Index");
        }

        if (sucursalOrigenId == sucursalDestinoId)
        {
            TempData["Error"] = "La sucursal de origen y destino no pueden ser la misma";
            return RedirectToAction("Index");
        }

        if (cantidad <= 0)
        {
            TempData["Error"] = "La cantidad debe ser mayor a cero";
            return RedirectToAction("Index");
        }

        try
        {
            // Verificar stock disponible en sucursal origen
            var stockOrigen = await _stockService.ObtenerStockActualPorProductoAsync(productoId, sucursalOrigenId);
            if (stockOrigen == null || stockOrigen.Stock < cantidad)
            {
                TempData["Error"] = $"Stock insuficiente en la sucursal de origen. Stock disponible: {stockOrigen?.Stock ?? 0}";
                return RedirectToAction("Index");
            }

            // Crear movimientos de inventario para la transferencia usando el servicio
            await CrearMovimientoTransferencia(productoId, sucursalOrigenId, -cantidad, $"Transferencia a sucursal {sucursalDestinoId}. {observacion ?? ""}", usuarioIdActual.Value);
            await CrearMovimientoTransferencia(productoId, sucursalDestinoId, cantidad, $"Transferencia desde sucursal {sucursalOrigenId}. {observacion ?? ""}", usuarioIdActual.Value);

            TempData["Success"] = $"Transferencia realizada exitosamente: {cantidad} unidades del producto transferidas";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al realizar la transferencia: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}
