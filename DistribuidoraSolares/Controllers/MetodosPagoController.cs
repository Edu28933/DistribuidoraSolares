using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class MetodosPagoController : Controller
{
    private readonly IMetodoPagoService _metodoPagoService;

    public MetodosPagoController(IMetodoPagoService metodoPagoService)
    {
        _metodoPagoService = metodoPagoService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<MetodoPago> metodosPago;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                metodosPago = await _metodoPagoService.BuscarMetodosPagoAsync(buscar);
            }
            else
            {
                metodosPago = await _metodoPagoService.MostrarMetodosPagoAsync();
            }
        }
        catch (Exception ex)
        {
            metodosPago = new List<MetodoPago>();
            TempData["Error"] = $"Error al cargar métodos de pago: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(metodosPago);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MetodoPago metodoPago)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _metodoPagoService.CrearMetodoPagoAsync(metodoPago);
                TempData["Success"] = "Método de pago creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el método de pago: {ex.Message}";
            }
        }
        
        return View(metodoPago);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var metodosPago = await _metodoPagoService.MostrarMetodosPagoAsync();
            var metodoPago = metodosPago.FirstOrDefault(m => m.MetodoPagoId == id);
            
            if (metodoPago == null)
            {
                return NotFound();
            }

            return View(metodoPago);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el método de pago: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MetodoPago metodoPago)
    {
        if (id != metodoPago.MetodoPagoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _metodoPagoService.EditarMetodoPagoAsync(metodoPago);
                TempData["Success"] = "Método de pago actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el método de pago: {ex.Message}";
            }
        }

        return View(metodoPago);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _metodoPagoService.EliminarMetodoPagoAsync(id);
            TempData["Success"] = "Método de pago eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el método de pago: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
