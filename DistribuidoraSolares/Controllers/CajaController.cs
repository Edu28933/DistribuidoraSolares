using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class CajaController : Controller
{
    private readonly ICajaService _cajaService;

    public CajaController(ICajaService cajaService)
    {
        _cajaService = cajaService;
    }

    public async Task<IActionResult> Index()
    {
        // Obtener información de sesión para filtrar por sucursal
        var rolNombre = HttpContext.Session.GetString("RolNombre");
        var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
        var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
        
        var cajas = await _cajaService.MostrarCajasAsync(sucursalIdFiltro);
        var cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();
        
        ViewBag.CajaAbierta = cajaAbierta;
        return View(cajas);
    }

    public IActionResult Abrir()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(decimal saldoInicial)
    {
        try
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
            {
                TempData["Error"] = "Debe iniciar sesión para abrir una caja";
                return RedirectToAction("Index", "Login");
            }

            // Obtener SucursalId del usuario (no usar SucursalHelper aquí porque queremos la sucursal del usuario, no el filtro)
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            // Si el usuario no tiene sucursal asignada (SuperAdmin/Contador), pasar null
            var sucursalId = sucursalIdUsuario.HasValue && sucursalIdUsuario.Value > 0 ? sucursalIdUsuario : null;
            
            var cajaId = await _cajaService.AbrirCajaAsync(saldoInicial, usuarioId.Value, sucursalId);
            TempData["Success"] = $"Caja #{cajaId} abierta exitosamente con saldo inicial de Q{saldoInicial:N2}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al abrir la caja: {ex.Message}";
            return View();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar()
    {
        try
        {
            var result = await _cajaService.CerrarCajaAsync();
            TempData["Success"] = $"Caja #{result.CajaId} cerrada exitosamente. Saldo final: Q{result.SaldoFinal:N2}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cerrar la caja: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}