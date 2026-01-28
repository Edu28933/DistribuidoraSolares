using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Controllers;

public class MovimientosInventarioController : Controller
{
    private readonly IInventarioMovimientoService _movimientoService;
    private readonly IProductoService _productoService;
    private readonly ISucursalService _sucursalService;

    public MovimientosInventarioController(
        IInventarioMovimientoService movimientoService,
        IProductoService productoService,
        ISucursalService sucursalService)
    {
        _movimientoService = movimientoService;
        _productoService = productoService;
        _sucursalService = sucursalService;
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> Index(string? buscar, int? productoId, int? sucursalIdFiltro, int? pagina, int? tamañoPagina)
    {
        List<InventarioMovimiento> movimientos = new List<InventarioMovimiento>();
        
        try
        {
            // Obtener información de sesión para filtrar por sucursal
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            
            // Si es SuperAdmin y no especificó sucursal, usar la del usuario
            // Si no es SuperAdmin, usar la del usuario
            int? sucursalIdFiltroFinal = null;
            if (EsSuperAdmin())
            {
                // SuperAdmin puede seleccionar sucursal, pero si no especificó, usar la del usuario
                sucursalIdFiltroFinal = sucursalIdFiltro ?? SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            }
            else
            {
                // No SuperAdmin: solo puede ver su sucursal
                sucursalIdFiltroFinal = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            }
            
            if (productoId.HasValue && productoId.Value > 0)
            {
                movimientos = await _movimientoService.ObtenerMovimientosPorProductoAsync(productoId.Value, sucursalIdFiltroFinal);
            }
            else if (!string.IsNullOrEmpty(buscar))
            {
                movimientos = await _movimientoService.BuscarMovimientosAsync(buscar, sucursalIdFiltroFinal);
            }
            else
            {
                movimientos = await _movimientoService.MostrarMovimientosAsync(sucursalIdFiltroFinal);
            }

            // Aplicar paginación
            var paginaActual = Helpers.PaginacionHelper.ObtenerPaginaActual(pagina);
            var tamañoPaginaActual = Helpers.PaginacionHelper.ObtenerTamañoPagina(tamañoPagina, 20);
            var totalItems = movimientos.Count;
            var (skip, take) = Helpers.PaginacionHelper.CalcularSkipTake(paginaActual, tamañoPaginaActual);
            
            movimientos = movimientos
                .OrderByDescending(m => m.FechaHora)
                .Skip(skip)
                .Take(take)
                .ToList();
            
            // Calcular totales de compras y ventas
            var movimientosCompletos = await _movimientoService.MostrarMovimientosAsync(sucursalIdFiltroFinal);
            var totalCompras = movimientosCompletos
                .Where(m => m.Tipo == "COMPRA" && m.Estado == "ACTIVO" && m.CostoUnitario.HasValue)
                .Sum(m => m.Cantidad * m.CostoUnitario.Value);
            var totalVentas = movimientosCompletos
                .Where(m => m.Tipo == "VENTA" && m.Estado == "ACTIVO" && m.CostoUnitario.HasValue)
                .Sum(m => Math.Abs(m.Cantidad) * m.CostoUnitario.Value);

            ViewBag.Paginacion = new Helpers.PaginacionResult<InventarioMovimiento>
            {
                Items = movimientos,
                PaginaActual = paginaActual,
                TamañoPagina = tamañoPaginaActual,
                TotalItems = totalItems
            };
            ViewBag.TotalCompras = totalCompras;
            ViewBag.TotalVentas = totalVentas;

            var productos = await _productoService.MostrarProductosAsync();
            ViewBag.Productos = productos;
            
            // Cargar sucursales para el filtro (solo si es SuperAdmin)
            if (EsSuperAdmin())
            {
                var sucursales = await _sucursalService.MostrarSucursalesAsync();
                ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar movimientos: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        ViewBag.ProductoId = productoId;
        ViewBag.SucursalIdFiltro = sucursalIdFiltro;
        ViewBag.EsSuperAdmin = EsSuperAdmin();
        return View(movimientos);
    }
}
