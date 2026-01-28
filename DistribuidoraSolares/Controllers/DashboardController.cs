using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class DashboardController : Controller
{
    private readonly ICajaService _cajaService;
    private readonly IStockService _stockService;

    public DashboardController(ICajaService cajaService, IStockService stockService)
    {
        _cajaService = cajaService;
        _stockService = stockService;
    }

    public async Task<IActionResult> Index()
    {
        // Verificar si el usuario es "Usuario Nuevo" y redirigir a Productos
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        var esUsuarioNuevo = rolNombre.Equals("Usuario Nuevo", StringComparison.OrdinalIgnoreCase);
        
        if (esUsuarioNuevo)
        {
            return RedirectToAction("Index", "Productos");
        }

        try
        {
            var cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();
            var stockBajo = new List<Models.StockActual>();
            
            try
            {
                // Obtener SucursalId del usuario de la sesión para filtrar stock
                var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
                var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
                
                var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltro);
                stockBajo = stockGeneral.Where(s => s.Stock <= s.StockMinimo).ToList();
            }
            catch
            {
                // Si hay error al obtener stock, continuar sin mostrar stock bajo
            }
            
            ViewBag.CajaAbierta = cajaAbierta;
            ViewBag.StockBajo = stockBajo;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el dashboard: {ex.Message}";
        }
        
        return View();
    }
}