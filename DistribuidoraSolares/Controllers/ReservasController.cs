using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Controllers;

public class ReservasController : Controller
{
    private readonly IReservaService _reservaService;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IUsuarioService _usuarioService;
    private readonly IStockService _stockService;

    public ReservasController(
        IReservaService reservaService,
        IProductoService productoService,
        IClienteService clienteService,
        IUsuarioService usuarioService,
        IStockService stockService)
    {
        _reservaService = reservaService;
        _productoService = productoService;
        _clienteService = clienteService;
        _usuarioService = usuarioService;
        _stockService = stockService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Reserva> reservas = new List<Reserva>();
        
        try
        {
            // Obtener información de sesión para filtrar por sucursal
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            
            if (!string.IsNullOrEmpty(buscar))
            {
                reservas = await _reservaService.BuscarReservasAsync(buscar, sucursalIdFiltro);
            }
            else
            {
                reservas = await _reservaService.MostrarReservasAsync(sucursalIdFiltro);
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar reservas: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(reservas);
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var productos = await _productoService.MostrarProductosAsync();
            var clientes = await _clienteService.MostrarClientesAsync();
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            
            // Obtener SucursalId del usuario de la sesión para filtrar stock
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(
                HttpContext.Session.GetString("RolNombre"), 
                sucursalIdUsuario);
            
            var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltro);
            
            if (!clientes.Any(c => c.Estado == "ACTIVO"))
            {
                TempData["Error"] = "No hay clientes activos. Debe crear al menos un cliente antes de crear una reserva.";
                return RedirectToAction("Index");
            }

            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            if (usuarioActivo == null)
            {
                TempData["Error"] = "No hay usuarios activos. Debe crear al menos un usuario activo antes de crear una reserva.";
                return RedirectToAction("Index");
            }

            ViewBag.Productos = productos.Where(p => p.Estado == "DISPONIBLE").ToList();
            ViewBag.Clientes = clientes.Where(c => c.Estado == "ACTIVO").ToList();
            ViewBag.UsuarioId = usuarioActivo.UsuarioId;
            ViewBag.Stock = stockGeneral.ToDictionary(s => s.ProductoId, s => s.Stock);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos para la reserva: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservaCompletaRequest request)
    {
        try
        {
            if (request.Detalle == null || request.Detalle.Count == 0)
            {
                TempData["Error"] = "Debe agregar al menos un producto a la reserva.";
                return RedirectToAction(nameof(Create));
            }

            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == request.UsuarioId && u.Estado == "ACTIVO");
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no válido o inactivo.";
                return RedirectToAction(nameof(Create));
            }

            // Obtener SucursalId del usuario de la sesión (no usar SucursalHelper aquí porque queremos la sucursal del usuario, no el filtro)
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            // Si el usuario no tiene sucursal asignada (SuperAdmin/Contador), pasar null
            request.SucursalId = sucursalIdUsuario.HasValue && sucursalIdUsuario.Value > 0 ? sucursalIdUsuario : null;

            var result = await _reservaService.CrearReservaCompletaAsync(request);
            TempData["Success"] = $"Reserva #{result.ReservaId} creada exitosamente. Estado: {result.Estado}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear la reserva: {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id, string? motivo)
    {
        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            if (usuarioActivo == null)
            {
                TempData["Error"] = "No hay usuarios activos.";
                return RedirectToAction(nameof(Index));
            }

            await _reservaService.CancelarReservaAsync(id, usuarioActivo.UsuarioId, motivo);
            TempData["Success"] = "Reserva cancelada exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cancelar la reserva: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
