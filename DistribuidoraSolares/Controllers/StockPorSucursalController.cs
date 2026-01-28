using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class StockPorSucursalController : Controller
{
    private readonly IStockService _stockService;
    private readonly ISucursalService _sucursalService;

    public StockPorSucursalController(IStockService stockService, ISucursalService sucursalService)
    {
        _stockService = stockService;
        _sucursalService = sucursalService;
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("Super Admin", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IActionResult> Index(int? sucursalId)
    {
        var usuarioIdActual = HttpContext.Session.GetInt32("UsuarioId");
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "Solo el SuperAdmin puede ver el stock por sucursal";
            return RedirectToAction("Index", "Dashboard");
        }

        try
        {
            // Obtener todas las sucursales para el selector
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            var sucursalesActivas = sucursales.Where(s => s.Estado == "ACTIVO").ToList();

            // Obtener stock por sucursal
            // Si sucursalId es null, mostrar todas las sucursales
            var stockPorSucursal = await _stockService.ObtenerStockPorSucursalAsync(sucursalId);

            ViewBag.Sucursales = sucursalesActivas;
            ViewBag.SucursalIdSeleccionada = sucursalId;
            
            return View(stockPorSucursal);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar stock por sucursal: {ex.Message}";
            return View(new List<Models.StockPorSucursal>());
        }
    }
}
