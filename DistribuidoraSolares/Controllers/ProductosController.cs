using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class ProductosController : Controller
{
    private readonly IProductoService _productoService;
    private readonly ICategoriaService _categoriaService;
    private readonly IStockService _stockService;
    private readonly ICodigoProductoService _codigoProductoService;
    private readonly IProductoFotoService _productoFotoService;
    private readonly IUsuarioService _usuarioService;
    private readonly IAzureBlobStorageService _blobStorageService;

    public ProductosController(
        IProductoService productoService,
        ICategoriaService categoriaService,
        IStockService stockService,
        ICodigoProductoService codigoProductoService,
        IProductoFotoService productoFotoService,
        IUsuarioService usuarioService,
        IAzureBlobStorageService blobStorageService)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _stockService = stockService;
        _codigoProductoService = codigoProductoService;
        _productoFotoService = productoFotoService;
        _usuarioService = usuarioService;
        _blobStorageService = blobStorageService;
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        List<Producto> productos = new List<Producto>();
        Dictionary<int, string> fotosPrincipales = new Dictionary<int, string>();
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                productos = await _productoService.BuscarProductosAsync(buscar);
            }
            else
            {
                productos = await _productoService.MostrarProductosAsync();
            }

            // Obtener stock actual para cada producto
            // Obtener SucursalId del usuario de la sesión para filtrar stock
            var sucursalIdUsuario = HttpContext.Session.GetInt32("SucursalId");
            var sucursalIdFiltro = SucursalHelper.ObtenerSucursalIdParaFiltro(
                HttpContext.Session.GetString("RolNombre"), 
                sucursalIdUsuario);
            
            var stockGeneral = await _stockService.ObtenerStockActualGeneralAsync(sucursalIdFiltro);
            var stockDict = stockGeneral.ToDictionary(s => s.ProductoId, s => s.Stock);
            
            ViewBag.Stock = stockDict;

            // Obtener fotos principales de cada producto
            foreach (var producto in productos)
            {
                try
                {
                    var fotos = await _productoFotoService.ObtenerFotosPorProductoAsync(producto.ProductoId);
                    var fotoPrincipal = fotos.FirstOrDefault(f => f.EsPrincipal && f.Estado == "ACTIVO");
                    if (fotoPrincipal == null)
                    {
                        // Si no hay principal, tomar la primera activa
                        fotoPrincipal = fotos.FirstOrDefault(f => f.Estado == "ACTIVO");
                    }
                    
                    if (fotoPrincipal != null && !string.IsNullOrEmpty(fotoPrincipal.UrlFoto))
                    {
                        fotosPrincipales[producto.ProductoId] = fotoPrincipal.UrlFoto;
                    }
                }
                catch
                {
                    // Si hay error al obtener fotos, continuar sin imagen
                }
            }
            
            ViewBag.FotosPrincipales = fotosPrincipales;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar productos: {ex.Message}";
        }
        
        ViewBag.Buscar = buscar;
        return View(productos);
    }

    public async Task<IActionResult> Create()
    {
        try
        {
            var categorias = await _categoriaService.MostrarCategoriasAsync();
            var categoriasActivas = categorias.Where(c => c.Estado == "ACTIVO").ToList();
            
            if (!categoriasActivas.Any())
            {
                TempData["Error"] = "No hay categorías activas. Debe crear al menos una categoría antes de crear productos.";
                return RedirectToAction("Index", "Categorias");
            }
            
            ViewBag.Categorias = categoriasActivas;
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar categorías: {ex.Message}";
            return RedirectToAction("Index");
        }
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        try
        {
            // Generar código automáticamente si no se proporcionó
            if (string.IsNullOrWhiteSpace(producto.Codigo) && producto.CategoriaId > 0)
            {
                producto.Codigo = await _codigoProductoService.GenerarCodigoAsync(producto.CategoriaId);
            }

            if (ModelState.IsValid)
            {
                await _productoService.CrearProductoAsync(producto);
                TempData["Success"] = $"Producto creado exitosamente con código {producto.Codigo}";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al crear el producto: {ex.Message}";
        }
        
        var categorias = await _categoriaService.MostrarCategoriasAsync();
        ViewBag.Categorias = categorias.Where(c => c.Estado == "ACTIVO").ToList();
        return View(producto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var productos = await _productoService.MostrarProductosAsync();
            var producto = productos.FirstOrDefault(p => p.ProductoId == id);
            
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var categorias = await _categoriaService.MostrarCategoriasAsync();
            ViewBag.Categorias = categorias.Where(c => c.Estado == "ACTIVO").ToList();
            
            return View(producto);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el producto: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.ProductoId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _productoService.EditarProductoAsync(producto);
                TempData["Success"] = "Producto actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el producto: {ex.Message}";
            }
        }

        try
        {
            var categorias = await _categoriaService.MostrarCategoriasAsync();
            ViewBag.Categorias = categorias.Where(c => c.Estado == "ACTIVO").ToList();
        }
        catch
        {
            // Si falla cargar categorías, continuar sin ellas
        }
        
        return View(producto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _productoService.EliminarProductoAsync(id);
            TempData["Success"] = "Producto eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el producto: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GenerarCodigo(int categoriaId)
    {
        try
        {
            var codigo = await _codigoProductoService.GenerarCodigoAsync(categoriaId);
            return Json(new { codigo });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Imagenes(int id)
    {
        try
        {
            if (id <= 0)
            {
                TempData["Error"] = "ID de producto inválido";
                return RedirectToAction(nameof(Index));
            }

            var productos = await _productoService.MostrarProductosAsync();
            var producto = productos.FirstOrDefault(p => p.ProductoId == id);
            
            if (producto == null)
            {
                TempData["Error"] = $"Producto con ID {id} no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var fotos = await _productoFotoService.ObtenerFotosPorProductoAsync(id);
            ViewBag.Producto = producto;
            ViewBag.Fotos = fotos ?? new List<Models.ProductoFoto>();
            
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            ViewBag.UsuarioId = usuarioActivo?.UsuarioId ?? 1;
            
            return View();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar imágenes: {ex.Message}";
            // Log del error completo para debugging
            Console.WriteLine($"Error en Imagenes: {ex}");
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirImagen(int productoId, IFormFile archivo, bool esPrincipal = false)
    {
        try
        {
            if (archivo == null || archivo.Length == 0)
            {
                TempData["Error"] = "Debe seleccionar un archivo";
                return RedirectToAction(nameof(Imagenes), new { id = productoId });
            }

            // Validar extensión
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["Error"] = "Formato de archivo no permitido. Use: JPG, PNG, GIF o WEBP";
                return RedirectToAction(nameof(Imagenes), new { id = productoId });
            }

            // Validar tamaño (máximo 10 MB)
            if (archivo.Length > 10 * 1024 * 1024)
            {
                TempData["Error"] = "El archivo es demasiado grande. Tamaño máximo: 10 MB";
                return RedirectToAction(nameof(Imagenes), new { id = productoId });
            }

            // Generar nombre único para Azure Blob Storage
            var nombreArchivo = $"producto-{productoId}/{Guid.NewGuid()}{extension}";

            // Determinar content type
            var contentType = archivo.ContentType;
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
            }

            // Subir a Azure Blob Storage
            string urlFoto;
            using (var stream = archivo.OpenReadStream())
            {
                urlFoto = await _blobStorageService.SubirImagenAsync(stream, nombreArchivo, contentType);
            }

            // Guardar URL en base de datos
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuarioActivo = usuarios.FirstOrDefault(u => u.Estado == "ACTIVO");
            var usuarioId = usuarioActivo?.UsuarioId ?? 1;

            await _productoFotoService.SubirFotoAsync(productoId, urlFoto, esPrincipal, usuarioId);
            
            TempData["Success"] = "Imagen subida exitosamente a Azure Blob Storage";
            return RedirectToAction(nameof(Imagenes), new { id = productoId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al subir la imagen: {ex.Message}";
            return RedirectToAction(nameof(Imagenes), new { id = productoId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarImagen(int fotoId, int productoId)
    {
        try
        {
            // Obtener la foto para eliminar el archivo de Azure Blob Storage
            var fotos = await _productoFotoService.ObtenerFotosPorProductoAsync(productoId);
            var foto = fotos.FirstOrDefault(f => f.FotoId == fotoId);
            
            if (foto != null && !string.IsNullOrEmpty(foto.UrlFoto))
            {
                // Eliminar de Azure Blob Storage
                try
                {
                    await _blobStorageService.EliminarImagenAsync(foto.UrlFoto);
                }
                catch (Exception ex)
                {
                    // Log del error pero continuar con la eliminación de BD
                    // En producción, deberías loggear esto
                    Console.WriteLine($"Error al eliminar imagen de Azure: {ex.Message}");
                }
            }

            await _productoFotoService.EliminarFotoAsync(fotoId);
            TempData["Success"] = "Imagen eliminada exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar la imagen: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Imagenes), new { id = productoId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarPrincipal(int fotoId, int productoId)
    {
        try
        {
            await _productoFotoService.MarcarComoPrincipalAsync(fotoId, productoId);
            TempData["Success"] = "Imagen marcada como principal";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al marcar la imagen como principal: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Imagenes), new { id = productoId });
    }
}