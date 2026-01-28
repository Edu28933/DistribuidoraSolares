using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class RolesController : Controller
{
    private readonly IRolService _rolService;

    public RolesController(IRolService rolService)
    {
        _rolService = rolService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Rol> roles;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                roles = await _rolService.BuscarRolesAsync(buscar);
            }
            else
            {
                roles = await _rolService.MostrarRolesAsync();
            }
        }
        catch (Exception ex)
        {
            roles = new List<Rol>();
            TempData["Error"] = $"Error al cargar roles: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(roles);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Rol rol)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _rolService.CrearRolAsync(rol);
                TempData["Success"] = "Rol creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el rol: {ex.Message}";
            }
        }
        
        return View(rol);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var roles = await _rolService.MostrarRolesAsync();
            var rol = roles.FirstOrDefault(r => r.RolId == id);
            
            if (rol == null)
            {
                return NotFound();
            }

            return View(rol);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el rol: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Rol rol)
    {
        if (id != rol.RolId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _rolService.EditarRolAsync(rol);
                TempData["Success"] = "Rol actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el rol: {ex.Message}";
            }
        }

        return View(rol);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _rolService.EliminarRolAsync(id);
            TempData["Success"] = "Rol eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el rol: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
