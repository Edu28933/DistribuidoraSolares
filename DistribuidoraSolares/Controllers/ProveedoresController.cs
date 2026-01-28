using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class ProveedoresController : Controller
{
    private readonly IProveedorService _proveedorService;

    public ProveedoresController(IProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Proveedor> proveedores;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                proveedores = await _proveedorService.BuscarProveedoresAsync(buscar);
            }
            else
            {
                proveedores = await _proveedorService.MostrarProveedoresAsync();
            }
        }
        catch (Exception ex)
        {
            proveedores = new List<Proveedor>();
            TempData["Error"] = $"Error al cargar proveedores: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(proveedores);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Proveedor proveedor)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _proveedorService.CrearProveedorAsync(proveedor);
                TempData["Success"] = "Proveedor creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el proveedor: {ex.Message}";
            }
        }
        
        return View(proveedor);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var proveedores = await _proveedorService.MostrarProveedoresAsync();
            var proveedor = proveedores.FirstOrDefault(p => p.ProveedorId == id);
            
            if (proveedor == null)
            {
                return NotFound();
            }

            return View(proveedor);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el proveedor: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Proveedor proveedor)
    {
        if (id != proveedor.ProveedorId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _proveedorService.EditarProveedorAsync(proveedor);
                TempData["Success"] = "Proveedor actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el proveedor: {ex.Message}";
            }
        }

        return View(proveedor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _proveedorService.EliminarProveedorAsync(id);
            TempData["Success"] = "Proveedor eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el proveedor: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
