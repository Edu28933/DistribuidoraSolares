using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class CategoriasController : Controller
{
    private readonly ICategoriaService _categoriaService;

    public CategoriasController(ICategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Categoria> categorias;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                categorias = await _categoriaService.BuscarCategoriasAsync(buscar);
            }
            else
            {
                categorias = await _categoriaService.MostrarCategoriasAsync();
            }
        }
        catch (Exception ex)
        {
            categorias = new List<Categoria>();
            TempData["Error"] = $"Error al cargar categorías: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(categorias);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _categoriaService.CrearCategoriaAsync(categoria);
                TempData["Success"] = "Categoría creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear la categoría: {ex.Message}";
            }
        }
        
        return View(categoria);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var categorias = await _categoriaService.MostrarCategoriasAsync();
            var categoria = categorias.FirstOrDefault(c => c.CategoriaId == id);
            
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la categoría: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.CategoriaId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _categoriaService.EditarCategoriaAsync(categoria);
                TempData["Success"] = "Categoría actualizada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar la categoría: {ex.Message}";
            }
        }

        return View(categoria);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoriaService.EliminarCategoriaAsync(id);
            TempData["Success"] = "Categoría eliminada exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar la categoría: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}