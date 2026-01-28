using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class PermisosController : Controller
{
    private readonly IPermisoPantallaService _permisoPantallaService;
    private readonly IRolService _rolService;

    public PermisosController(
        IPermisoPantallaService permisoPantallaService,
        IRolService rolService)
    {
        _permisoPantallaService = permisoPantallaService;
        _rolService = rolService;
    }

    public async Task<IActionResult> Index(int? rolId)
    {
        var pantallas = await _permisoPantallaService.ObtenerPantallasAsync();
        var roles = await _rolService.MostrarRolesAsync();

        ViewBag.Roles = roles.Where(r => r.Estado == "ACTIVO").ToList();
        ViewBag.Pantallas = pantallas;
        ViewBag.RolIdSeleccionado = rolId;

        List<PermisoPantalla> permisos = new();
        if (rolId.HasValue)
        {
            permisos = await _permisoPantallaService.ObtenerPermisosPorRolAsync(rolId.Value);
        }

        return View(permisos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuardarPermisos(int? rolId, IFormCollection form)
    {
        try
        {
            if (!rolId.HasValue)
            {
                TempData["Error"] = "Debe seleccionar un rol";
                return RedirectToAction(nameof(Index));
            }

            var pantallas = await _permisoPantallaService.ObtenerPantallasAsync();
            
            foreach (var pantalla in pantallas)
            {
                var permiso = new PermisoPantalla
                {
                    RolId = rolId.Value,
                    PantallaId = pantalla.PantallaId,
                    PuedeVer = form.ContainsKey($"permisos[{pantalla.PantallaId}].PuedeVer"),
                    PuedeCrear = form.ContainsKey($"permisos[{pantalla.PantallaId}].PuedeCrear"),
                    PuedeEditar = form.ContainsKey($"permisos[{pantalla.PantallaId}].PuedeEditar"),
                    PuedeEliminar = form.ContainsKey($"permisos[{pantalla.PantallaId}].PuedeEliminar")
                };

                await _permisoPantallaService.GuardarPermisoAsync(permiso);
            }

            TempData["Success"] = "Permisos guardados exitosamente";
            return RedirectToAction(nameof(Index), new { rolId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al guardar permisos: {ex.Message}";
            return RedirectToAction(nameof(Index), new { rolId });
        }
    }
}
