using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class SucursalesController : Controller
{
    private readonly ISucursalService _sucursalService;

    public SucursalesController(ISucursalService sucursalService)
    {
        _sucursalService = sucursalService;
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("SUPERADMIN", StringComparison.OrdinalIgnoreCase) ||
               rolNombre.Equals("Super Admin", StringComparison.OrdinalIgnoreCase);
    }

    private int? GetUsuarioId()
    {
        return HttpContext.Session.GetInt32("UsuarioId");
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        List<Sucursal> sucursales;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                sucursales = await _sucursalService.BuscarSucursalesAsync(buscar);
            }
            else
            {
                sucursales = await _sucursalService.MostrarSucursalesAsync();
            }

            ViewBag.Buscar = buscar;
            ViewBag.EsSuperAdmin = EsSuperAdmin();
            return View(sucursales);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar sucursales: {ex.Message}";
            return View(new List<Sucursal>());
        }
    }

    public IActionResult Create()
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para crear sucursales";
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sucursal sucursal)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para crear sucursales";
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(sucursal.Nombre))
        {
            TempData["Error"] = "El nombre de la sucursal es obligatorio";
            return View(sucursal);
        }

        try
        {
            var sucursalId = await _sucursalService.CrearSucursalAsync(sucursal);
            
            if (sucursalId > 0)
            {
                TempData["Success"] = "Sucursal creada exitosamente";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "No se pudo crear la sucursal";
                return View(sucursal);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear sucursal: {ex.Message}";
            return View(sucursal);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para editar sucursales";
            return RedirectToAction("Index");
        }

        try
        {
            var sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(id);
            
            if (sucursal == null)
            {
                TempData["Error"] = "Sucursal no encontrada";
                return RedirectToAction("Index");
            }

            return View(sucursal);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar sucursal: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Sucursal sucursal)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para editar sucursales";
            return RedirectToAction("Index");
        }

        if (string.IsNullOrWhiteSpace(sucursal.Nombre))
        {
            TempData["Error"] = "El nombre de la sucursal es obligatorio";
            return View(sucursal);
        }

        try
        {
            var resultado = await _sucursalService.EditarSucursalAsync(sucursal);
            
            if (resultado > 0)
            {
                TempData["Success"] = "Sucursal actualizada exitosamente";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar la sucursal";
                return View(sucursal);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al actualizar sucursal: {ex.Message}";
            return View(sucursal);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para eliminar sucursales";
            return RedirectToAction("Index");
        }

        try
        {
            var resultado = await _sucursalService.EliminarSucursalAsync(id);
            
            if (resultado > 0)
            {
                TempData["Success"] = "Sucursal eliminada exitosamente";
            }
            else
            {
                TempData["Error"] = "No se pudo eliminar la sucursal";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar sucursal: {ex.Message}";
        }

        return RedirectToAction("Index");
    }
}
