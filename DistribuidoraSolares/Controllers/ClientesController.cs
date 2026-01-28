using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class ClientesController : Controller
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Cliente> clientes;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                clientes = await _clienteService.BuscarClientesAsync(buscar);
            }
            else
            {
                clientes = await _clienteService.MostrarClientesAsync();
            }
        }
        catch (Exception ex)
        {
            clientes = new List<Cliente>();
            TempData["Error"] = $"Error al cargar clientes: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(clientes);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _clienteService.CrearClienteAsync(cliente);
                TempData["Success"] = "Cliente creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el cliente: {ex.Message}";
            }
        }
        
        return View(cliente);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var clientes = await _clienteService.MostrarClientesAsync();
            var cliente = clientes.FirstOrDefault(c => c.ClienteId == id);
            
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el cliente: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.ClienteId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _clienteService.EditarClienteAsync(cliente);
                TempData["Success"] = "Cliente actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el cliente: {ex.Message}";
            }
        }

        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _clienteService.EliminarClienteAsync(id);
            TempData["Success"] = "Cliente eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el cliente: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
