using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Controllers;

public class ComprasController : Controller
{
    private readonly ICompraService _compraService;
    private readonly IProductoService _productoService;
    private readonly IProveedorService _proveedorService;
    private readonly IUsuarioService _usuarioService;

    public ComprasController(
        ICompraService compraService,
        IProductoService productoService,
        IProveedorService proveedorService,
        IUsuarioService usuarioService)
    {
        _compraService = compraService;
        _productoService = productoService;
        _proveedorService = proveedorService;
        _usuarioService = usuarioService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Compra> compras = new List<Compra>();
        
        try
        {
            // Obtener información de sesión para filtrar por sucursal
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            
            if (!string.IsNullOrEmpty(buscar))
            {
                compras = await _compraService.BuscarComprasAsync(buscar, sucursalIdFiltro);
            }
            else
            {
                compras = await _compraService.MostrarComprasAsync(sucursalIdFiltro);
            }

            // Obtener proveedores para mostrar nombres
            var proveedores = await _proveedorService.MostrarProveedoresAsync();
            ViewBag.Proveedores = proveedores.ToDictionary(p => p.ProveedorId, p => p.Nombre);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar compras: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        return View(compras);
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var productos = await _productoService.MostrarProductosAsync();
            var proveedores = await _proveedorService.MostrarProveedoresAsync();
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            
            if (!proveedores.Any(p => p.Estado == "ACTIVO"))
            {
                TempData["Error"] = "No hay proveedores activos. Debe crear al menos un proveedor antes de registrar una compra.";
                return RedirectToAction("Index");
            }

            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            if (usuarioActivo == null)
            {
                TempData["Error"] = "No hay usuarios activos. Debe crear al menos un usuario activo antes de registrar una compra.";
                return RedirectToAction("Index");
            }

            ViewBag.Productos = productos.Where(p => p.Estado == "DISPONIBLE").ToList();
            ViewBag.Proveedores = proveedores.Where(p => p.Estado == "ACTIVO").ToList();
            ViewBag.UsuarioId = usuarioActivo.UsuarioId;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos para la compra: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompraCompletaRequest request)
    {
        try
        {
            if (request.Detalle == null || request.Detalle.Count == 0)
            {
                TempData["Error"] = "Debe agregar al menos un producto a la compra.";
                return RedirectToAction(nameof(Create));
            }

            // Validar que el usuario existe y está activo
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == request.UsuarioId && u.Estado == "ACTIVO");
            if (usuario == null)
            {
                TempData["Error"] = "Usuario no válido o inactivo. Debe seleccionar un usuario activo.";
                return RedirectToAction(nameof(Create));
            }

            // Obtener SucursalId del usuario de la sesión (no usar SucursalHelper aquí porque queremos la sucursal del usuario, no el filtro)
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            // Si el usuario no tiene sucursal asignada (SuperAdmin/Contador), pasar null
            request.SucursalId = sucursalIdUsuario.HasValue && sucursalIdUsuario.Value > 0 ? sucursalIdUsuario : null;

            var result = await _compraService.CrearCompraCompletaAsync(request);
            TempData["Success"] = $"Compra #{result.CompraId} registrada exitosamente. Total: Q{result.Total:N2}. El stock ha sido actualizado.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al registrar la compra: {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }
}
