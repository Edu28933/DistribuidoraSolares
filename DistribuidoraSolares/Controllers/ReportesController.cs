using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Helpers;

namespace DistribuidoraSolares.Controllers;

public class ReportesController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly ICompraService _compraService;
    private readonly IInventarioMovimientoService _movimientoService;
    private readonly IProductoService _productoService;
    private readonly ISucursalService _sucursalService;
    private readonly IClienteService _clienteService;
    private readonly IStockService _stockService;

    public ReportesController(
        IVentaService ventaService,
        ICompraService compraService,
        IInventarioMovimientoService movimientoService,
        IProductoService productoService,
        ISucursalService sucursalService,
        IClienteService clienteService,
        IStockService stockService)
    {
        _ventaService = ventaService;
        _compraService = compraService;
        _movimientoService = movimientoService;
        _productoService = productoService;
        _sucursalService = sucursalService;
        _clienteService = clienteService;
        _stockService = stockService;
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> Index()
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            // Cargar datos básicos para los reportes
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            ViewBag.EsSuperAdmin = EsSuperAdmin();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos: {ex.Message}";
        }

        return View();
    }

    // Reporte de Ventas por Período
    public async Task<IActionResult> VentasPorPeriodo(DateTime? fechaDesde, DateTime? fechaHasta, int? sucursalId)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = EsSuperAdmin() && sucursalId.HasValue 
                ? sucursalId 
                : SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);

            var ventas = await _ventaService.MostrarVentasAsync(sucursalIdFiltro);

            if (fechaDesde.HasValue)
            {
                ventas = ventas.Where(v => v.FechaHora >= fechaDesde.Value).ToList();
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaConHora = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                ventas = ventas.Where(v => v.FechaHora <= fechaHastaConHora).ToList();
            }

            var totalVentas = ventas.Sum(v => v.TotalNeto);
            var cantidadVentas = ventas.Count;

            ViewBag.Ventas = ventas;
            ViewBag.TotalVentas = totalVentas;
            ViewBag.CantidadVentas = cantidadVentas;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.SucursalId = sucursalId;

            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    // Reporte de Productos Más Vendidos
    public async Task<IActionResult> ProductosMasVendidos(DateTime? fechaDesde, DateTime? fechaHasta, int? sucursalId, int? topN)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = EsSuperAdmin() && sucursalId.HasValue 
                ? sucursalId 
                : SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);

            var movimientos = await _movimientoService.MostrarMovimientosAsync(sucursalIdFiltro);
            
            var ventasMovimientos = movimientos
                .Where(m => m.Tipo == "VENTA" && m.Estado == "ACTIVO")
                .ToList();

            if (fechaDesde.HasValue)
            {
                ventasMovimientos = ventasMovimientos.Where(m => m.FechaHora >= fechaDesde.Value).ToList();
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaConHora = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                ventasMovimientos = ventasMovimientos.Where(m => m.FechaHora <= fechaHastaConHora).ToList();
            }

            var productosVendidos = ventasMovimientos
                .Where(m => m.CostoUnitario.HasValue)
                .GroupBy(m => new { m.ProductoId, m.Producto?.Nombre })
                .Select(g => new ReporteProductoVendido
                {
                    ProductoId = g.Key.ProductoId,
                    ProductoNombre = g.Key.Nombre ?? $"Producto {g.Key.ProductoId}",
                    CantidadVendida = g.Sum(m => Math.Abs(m.Cantidad)),
                    TotalVendido = g.Sum(m => Math.Abs(m.Cantidad) * m.CostoUnitario.Value)
                })
                .OrderByDescending(p => p.TotalVendido)
                .Take(topN ?? 10)
                .ToList();

            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.SucursalId = sucursalId;
            ViewBag.TopN = topN ?? 10;

            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            
            return View(productosVendidos);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    // Reporte de Stock Bajo
    public async Task<IActionResult> StockBajo(int? sucursalId)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = EsSuperAdmin() && sucursalId.HasValue 
                ? sucursalId 
                : SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);

            var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltro);
            var stockBajo = stockGeneral.Where(s => s.Stock <= s.StockMinimo).OrderBy(s => s.Stock).ToList();

            ViewBag.StockBajo = stockBajo;
            ViewBag.SucursalId = sucursalId;

            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    // Reporte de Rentabilidad (Ventas vs Compras)
    public async Task<IActionResult> Rentabilidad(DateTime? fechaDesde, DateTime? fechaHasta, int? sucursalId)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = EsSuperAdmin() && sucursalId.HasValue 
                ? sucursalId 
                : SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);

            var movimientos = await _movimientoService.MostrarMovimientosAsync(sucursalIdFiltro);

            if (fechaDesde.HasValue)
            {
                movimientos = movimientos.Where(m => m.FechaHora >= fechaDesde.Value).ToList();
            }

            if (fechaHasta.HasValue)
            {
                var fechaHastaConHora = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                movimientos = movimientos.Where(m => m.FechaHora <= fechaHastaConHora).ToList();
            }

            var totalCompras = movimientos
                .Where(m => m.Tipo == "COMPRA" && m.Estado == "ACTIVO" && m.CostoUnitario.HasValue)
                .Sum(m => m.Cantidad * m.CostoUnitario.Value);

            var totalVentas = movimientos
                .Where(m => m.Tipo == "VENTA" && m.Estado == "ACTIVO" && m.CostoUnitario.HasValue)
                .Sum(m => Math.Abs(m.Cantidad) * m.CostoUnitario.Value);

            var ganancia = totalVentas - totalCompras;
            var margenGanancia = totalCompras > 0 ? (ganancia / totalCompras) * 100 : 0;

            ViewBag.TotalCompras = totalCompras;
            ViewBag.TotalVentas = totalVentas;
            ViewBag.Ganancia = ganancia;
            ViewBag.MargenGanancia = margenGanancia;
            ViewBag.FechaDesde = fechaDesde;
            ViewBag.FechaHasta = fechaHasta;
            ViewBag.SucursalId = sucursalId;

            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    // Reporte de Clientes
    public async Task<IActionResult> Clientes(int? sucursalId)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = EsSuperAdmin() && sucursalId.HasValue 
                ? sucursalId 
                : SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);

            var ventas = await _ventaService.MostrarVentasAsync(sucursalIdFiltro);
            var clientes = await _clienteService.MostrarClientesAsync();

            var clientesConVentas = clientes
                .Where(c => c.Estado == "ACTIVO")
                .Select(c => new ReporteCliente
                {
                    Cliente = c,
                    CantidadVentas = ventas.Count(v => v.ClienteId == c.ClienteId),
                    TotalCompras = ventas.Where(v => v.ClienteId == c.ClienteId).Sum(v => v.TotalNeto)
                })
                .Where(c => c.CantidadVentas > 0)
                .OrderByDescending(c => c.TotalCompras)
                .ToList();

            return View(clientesConVentas);
            ViewBag.SucursalId = sucursalId;

            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar el reporte: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }
}
