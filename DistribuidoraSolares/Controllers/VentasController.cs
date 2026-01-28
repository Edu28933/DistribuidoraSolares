using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;

namespace DistribuidoraSolares.Controllers;

public class VentasController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly IProductoService _productoService;
    private readonly IClienteService _clienteService;
    private readonly IMetodoPagoService _metodoPagoService;
    private readonly ICajaService _cajaService;
    private readonly IStockService _stockService;
    private readonly IUsuarioService _usuarioService;
    private readonly IReservaService _reservaService;

    public VentasController(
        IVentaService ventaService,
        IProductoService productoService,
        IClienteService clienteService,
        IMetodoPagoService metodoPagoService,
        ICajaService cajaService,
        IStockService stockService,
        IUsuarioService usuarioService,
        IReservaService reservaService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
        _clienteService = clienteService;
        _metodoPagoService = metodoPagoService;
        _cajaService = cajaService;
        _stockService = stockService;
        _usuarioService = usuarioService;
        _reservaService = reservaService;
    }

    public async Task<IActionResult> Index(
        string? buscar, 
        int? clienteId, 
        int? productoId, 
        decimal? montoMin, 
        decimal? montoMax, 
        DateTime? fechaDesde, 
        DateTime? fechaHasta,
        int? pagina,
        int? tamañoPagina)
    {
        List<Venta> ventas = new List<Venta>();
        
        try
        {
            // Obtener información de sesión para filtrar por sucursal
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            
            // Obtener todas las ventas primero
            if (!string.IsNullOrEmpty(buscar))
            {
                ventas = await _ventaService.BuscarVentasAsync(buscar, sucursalIdFiltro);
            }
            else
            {
                ventas = await _ventaService.MostrarVentasAsync(sucursalIdFiltro);
            }
            
            // Aplicar filtros adicionales
            if (clienteId.HasValue)
            {
                ventas = ventas.Where(v => v.ClienteId == clienteId.Value).ToList();
            }
            
            if (productoId.HasValue)
            {
                // Necesitamos cargar los detalles para filtrar por producto
                var ventasConProducto = new List<Venta>();
                foreach (var venta in ventas)
                {
                    var ventaCompleta = await _ventaService.ObtenerVentaPorIdAsync(venta.VentaId);
                    if (ventaCompleta?.Detalles?.Any(d => d.ProductoId == productoId.Value) == true)
                    {
                        ventasConProducto.Add(venta);
                    }
                }
                ventas = ventasConProducto;
            }
            
            if (montoMin.HasValue)
            {
                ventas = ventas.Where(v => v.TotalNeto >= montoMin.Value).ToList();
            }
            
            if (montoMax.HasValue)
            {
                ventas = ventas.Where(v => v.TotalNeto <= montoMax.Value).ToList();
            }
            
            if (fechaDesde.HasValue)
            {
                ventas = ventas.Where(v => v.FechaHora >= fechaDesde.Value).ToList();
            }
            
            if (fechaHasta.HasValue)
            {
                var fechaHastaConHora = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                ventas = ventas.Where(v => v.FechaHora <= fechaHastaConHora).ToList();
            }
            
            // Aplicar paginación
            var paginaActual = Helpers.PaginacionHelper.ObtenerPaginaActual(pagina);
            var tamañoPaginaActual = Helpers.PaginacionHelper.ObtenerTamañoPagina(tamañoPagina, 20);
            var totalItems = ventas.Count;
            var (skip, take) = Helpers.PaginacionHelper.CalcularSkipTake(paginaActual, tamañoPaginaActual);
            
            ventas = ventas
                .OrderByDescending(v => v.FechaHora)
                .Skip(skip)
                .Take(take)
                .ToList();
            
            ViewBag.Paginacion = new Helpers.PaginacionResult<Venta>
            {
                Items = ventas,
                PaginaActual = paginaActual,
                TamañoPagina = tamañoPaginaActual,
                TotalItems = totalItems
            };
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar ventas: {ex.Message}";
        }

        // Cargar datos para los filtros
        var clientes = await _clienteService.MostrarClientesAsync();
        var productos = await _productoService.MostrarProductosAsync();
        
        ViewBag.Buscar = buscar;
        ViewBag.Clientes = clientes.Where(c => c.Estado == "ACTIVO").ToList();
        ViewBag.Productos = productos.Where(p => p.Estado == "DISPONIBLE").ToList();
        ViewBag.ClienteIdFiltro = clienteId;
        ViewBag.ProductoIdFiltro = productoId;
        ViewBag.MontoMinFiltro = montoMin;
        ViewBag.MontoMaxFiltro = montoMax;
        ViewBag.FechaDesdeFiltro = fechaDesde;
        ViewBag.FechaHastaFiltro = fechaHasta;
        
        return View(ventas);
    }
    
    public async Task<IActionResult> Detalle(int id)
    {
        try
        {
            var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }
            
            return View(venta);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el detalle de la venta: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var cajaAbierta = await _cajaService.ObtenerCajaAbiertaAsync();
            if (cajaAbierta == null)
            {
                TempData["Error"] = "Debe abrir una caja antes de realizar una venta.";
                return RedirectToAction("Index", "Caja");
            }

            var productos = await _productoService.MostrarProductosAsync();
            var productosDisponibles = productos.Where(p => p.Estado == "DISPONIBLE").ToList();
            
            if (!productosDisponibles.Any())
            {
                TempData["Error"] = "No hay productos disponibles. Debe crear productos antes de realizar una venta.";
                return RedirectToAction("Index", "Productos");
            }

            var clientes = await _clienteService.MostrarClientesAsync();
            var metodosPago = await _metodoPagoService.MostrarMetodosPagoAsync();
            
            // Obtener SucursalId del usuario de la sesión
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var esVendedor = rolNombre?.Equals("Vendedor", StringComparison.OrdinalIgnoreCase) == true;
            
            // Vendedores pueden ver stock de todas las sucursales para poder vender
            // Otros roles ven solo su sucursal
            int? sucursalIdFiltroStock = null;
            if (!esVendedor)
            {
                sucursalIdFiltroStock = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            }
            
            // Stock de la sucursal del usuario (o todas si es Vendedor)
            var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltroStock);
            
            // Si es Vendedor, también cargar stock por sucursal para mostrar opciones
            Dictionary<int, Dictionary<int, int>>? stockPorSucursal = null;
            if (esVendedor)
            {
                var stockPorSucursalData = await _stockService.ObtenerStockPorSucursalAsync(null);
                stockPorSucursal = stockPorSucursalData
                    .GroupBy(s => s.ProductoId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToDictionary(s => s.SucursalId ?? 0, s => s.Stock)
                    );
            }
            
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            
            if (usuarioActivo == null)
            {
                TempData["Error"] = "No hay usuarios activos. Debe crear al menos un usuario activo antes de realizar una venta.";
                return RedirectToAction("Index");
            }

            ViewBag.Productos = productosDisponibles;
            ViewBag.Clientes = clientes.Where(c => c.Estado == "ACTIVO").ToList();
            ViewBag.MetodosPago = metodosPago.Where(m => m.Estado == "ACTIVO").ToList();
            ViewBag.Stock = stockGeneral.ToDictionary(s => s.ProductoId, s => s.Stock);
            ViewBag.StockPorSucursal = stockPorSucursal;
            ViewBag.EsVendedor = esVendedor;
            ViewBag.SucursalIdUsuario = sucursalIdUsuario;
            ViewBag.UsuarioId = usuarioActivo.UsuarioId;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos para la venta: {ex.Message}";
            return RedirectToAction("Index");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VentaCompletaRequest request)
    {
        try
        {
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

            // Si la venta viene de una reserva, LIBERAR los productos ANTES de crear la venta
            // Esto asegura que el stock físico esté correcto cuando el stored procedure valide
            if (request.ReservaId.HasValue)
            {
                var detallesVenta = request.Detalle.Select(d => new DistribuidoraSolares.Services.VentaDetalleReserva
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad
                }).ToList();
                
                // 1. Liberar los productos de reserva ANTES de crear la venta
                // Esto crea movimientos LIBERACION_RESERVA que compensan el movimiento RESERVA original
                // El stock físico quedará correcto para que el stored procedure pueda validar
                await _reservaService.LiberarProductosReservaParaVentaAsync(
                    request.ReservaId.Value, 
                    0, // VentaId temporal (se actualizará después)
                    request.UsuarioId, 
                    detallesVenta
                );
            }

            // 2. Crear la venta (ahora el stock ya está liberado, así que pasará la validación)
            var result = await _ventaService.CrearVentaCompletaAsync(request);
            
            // 3. Si la venta viene de una reserva, actualizar los movimientos y marcar detalles
            if (request.ReservaId.HasValue)
            {
                var detallesVenta = request.Detalle.Select(d => new DistribuidoraSolares.Services.VentaDetalleReserva
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad
                }).ToList();
                
                // Actualizar los movimientos LIBERACION_RESERVA con el VentaId correcto
                await _reservaService.ActualizarMovimientosLiberacionConVentaIdAsync(
                    request.ReservaId.Value,
                    result.VentaId
                );
                
                // Marcar solo los detalles de reserva que fueron vendidos como ocupados
                await _reservaService.MarcarDetallesReservaComoOcupadosAsync(request.ReservaId.Value, detallesVenta);
                
                // Marcar la reserva como completada
                await _reservaService.MarcarReservaComoCompletadaAsync(request.ReservaId.Value);
            }

            TempData["Success"] = $"Venta #{result.VentaId} creada exitosamente. Total: Q{result.TotalNeto:N2}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear la venta: {ex.Message}";
            return RedirectToAction(nameof(Create));
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerReservasActivas(int clienteId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"=== ObtenerReservasActivas llamado con clienteId: {clienteId} ===");
            
            if (clienteId <= 0)
            {
                System.Diagnostics.Debug.WriteLine("clienteId inválido");
                return Json(new List<ReservaConDetalles>());
            }
            
            var reservas = await _reservaService.ObtenerReservasActivasPorClienteAsync(clienteId);
            System.Diagnostics.Debug.WriteLine($"Reservas encontradas: {reservas?.Count ?? 0}");
            
            if (reservas != null && reservas.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Primera reserva ID: {reservas.First().ReservaId}, Detalles: {reservas.First().Detalles?.Count ?? 0}");
            }
            
            return Json(reservas ?? new List<ReservaConDetalles>(), new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = null // Mantener PascalCase
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR en ObtenerReservasActivas: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return Json(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (venta.Estado != "ACTIVO")
            {
                TempData["Error"] = "No se puede editar una venta anulada";
                return RedirectToAction(nameof(Index));
            }

            var productos = await _productoService.MostrarProductosAsync();
            var productosDisponibles = productos.Where(p => p.Estado == "DISPONIBLE").ToList();
            var clientes = await _clienteService.MostrarClientesAsync();
            var metodosPago = await _metodoPagoService.MostrarMetodosPagoAsync();
            
            // Obtener SucursalId del usuario de la sesión
            var rolNombre = HttpContext.Session.GetString("RolNombre");
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var esVendedor = rolNombre?.Equals("Vendedor", StringComparison.OrdinalIgnoreCase) == true;
            
            // Vendedores pueden ver stock de todas las sucursales
            int? sucursalIdFiltroStock = null;
            if (!esVendedor)
            {
                sucursalIdFiltroStock = SucursalHelper.ObtenerSucursalIdParaFiltro(rolNombre, sucursalIdUsuario);
            }
            
            var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltroStock);
            
            // Si es Vendedor, también cargar stock por sucursal
            Dictionary<int, Dictionary<int, int>>? stockPorSucursal = null;
            if (esVendedor)
            {
                var stockPorSucursalData = await _stockService.ObtenerStockPorSucursalAsync(null);
                stockPorSucursal = stockPorSucursalData
                    .GroupBy(s => s.ProductoId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.ToDictionary(s => s.SucursalId ?? 0, s => s.Stock)
                    );
            }
            
            var usuarios = await _usuarioService.MostrarUsuariosAsync();

            ViewBag.Productos = productosDisponibles;
            ViewBag.Clientes = clientes.Where(c => c.Estado == "ACTIVO").ToList();
            ViewBag.MetodosPago = metodosPago.Where(m => m.Estado == "ACTIVO").ToList();
            ViewBag.Stock = stockGeneral.ToDictionary(s => s.ProductoId, s => s.Stock);
            ViewBag.StockPorSucursal = stockPorSucursal;
            ViewBag.EsVendedor = esVendedor;
            ViewBag.SucursalIdUsuario = sucursalIdUsuario;
            ViewBag.UsuarioId = venta.UsuarioId;
            ViewBag.Venta = venta;

            return View(venta);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la venta: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VentaCompletaRequest request, string observacionEdicion)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(observacionEdicion))
            {
                TempData["Error"] = "Debe proporcionar una observación explicando la razón de la edición";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var result = await _ventaService.EditarVentaAsync(id, request, observacionEdicion);
            TempData["Success"] = $"Venta editada exitosamente. Nueva venta #{result.VentaId} creada. Total: Q{result.TotalNeto:N2}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al editar la venta: {ex.Message}";
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    public async Task<IActionResult> ImprimirFactura(int id)
    {
        try
        {
            var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            // Por ahora retornamos una vista con los datos de la factura
            // Más adelante podemos implementar generación de PDF real
            return View("Factura", venta);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar factura: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id, string motivoAnulacion)
    {
        try
        {
            var venta = await _ventaService.ObtenerVentaPorIdAsync(id);
            if (venta == null)
            {
                TempData["Error"] = "Venta no encontrada";
                return RedirectToAction(nameof(Index));
            }

            if (venta.Estado != "ACTIVO")
            {
                TempData["Error"] = "Solo se pueden anular ventas activas";
                return RedirectToAction(nameof(Index));
            }

            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            if (usuarioActivo == null)
            {
                TempData["Error"] = "No hay usuarios activos";
                return RedirectToAction(nameof(Index));
            }

            await _ventaService.AnularVentaAsync(id, usuarioActivo.UsuarioId, motivoAnulacion);
            TempData["Success"] = $"Venta #{id} anulada exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al anular la venta: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}