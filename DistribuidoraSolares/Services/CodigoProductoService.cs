using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DistribuidoraSolares.Services;

public interface ICodigoProductoService
{
    Task<string> GenerarCodigoAsync(int categoriaId);
}

public class CodigoProductoService : ICodigoProductoService
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoriaService _categoriaService;
    private readonly IProductoService _productoService;

    public CodigoProductoService(
        ApplicationDbContext context,
        ICategoriaService categoriaService,
        IProductoService productoService)
    {
        _context = context;
        _categoriaService = categoriaService;
        _productoService = productoService;
    }

    public async Task<string> GenerarCodigoAsync(int categoriaId)
    {
        // Obtener la categoría
        var categorias = await _categoriaService.MostrarCategoriasAsync();
        var categoria = categorias.FirstOrDefault(c => c.CategoriaId == categoriaId);
        
        if (categoria == null)
        {
            throw new ArgumentException("Categoría no encontrada");
        }

        // Generar prefijo del código basado en el nombre de la categoría
        // Ejemplo: "Sublimación" -> "sub"
        var prefijo = GenerarPrefijo(categoria.Nombre);

        // Obtener todos los productos de esta categoría para encontrar el siguiente número
        var productos = await _productoService.MostrarProductosAsync();
        var productosCategoria = productos.Where(p => p.CategoriaId == categoriaId && p.Codigo.StartsWith(prefijo + "_")).ToList();

        // Encontrar el siguiente número disponible
        int siguienteNumero = 1;
        if (productosCategoria.Any())
        {
            var numeros = productosCategoria
                .Select(p => {
                    var partes = p.Codigo.Split('_');
                    if (partes.Length > 1 && int.TryParse(partes[1], out int num))
                        return num;
                    return 0;
                })
                .Where(n => n > 0)
                .ToList();

            if (numeros.Any())
            {
                siguienteNumero = numeros.Max() + 1;
            }
        }

        // Formatear el número con ceros a la izquierda (001, 002, etc.)
        return $"{prefijo}_{siguienteNumero:D3}";
    }

    private string GenerarPrefijo(string nombreCategoria)
    {
        // Normalizar el nombre: quitar acentos, convertir a minúsculas
        var texto = nombreCategoria.ToLower().Trim();
        
        // Remover acentos y caracteres especiales
        texto = Regex.Replace(texto, @"[áàäâ]", "a");
        texto = Regex.Replace(texto, @"[éèëê]", "e");
        texto = Regex.Replace(texto, @"[íìïî]", "i");
        texto = Regex.Replace(texto, @"[óòöô]", "o");
        texto = Regex.Replace(texto, @"[úùüû]", "u");
        texto = Regex.Replace(texto, @"[ñ]", "n");
        
        // Remover caracteres especiales y espacios
        texto = Regex.Replace(texto, @"[^a-z0-9\s]", "");
        
        // Tomar las primeras 3-4 letras significativas
        var palabras = texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (palabras.Length == 1)
        {
            // Si es una sola palabra, tomar las primeras 3 letras (ej: "sublimacion" -> "sub")
            var palabra = palabras[0];
            if (palabra.Length >= 3)
            {
                return palabra.Substring(0, 3);
            }
            return palabra;
        }
        else
        {
            // Si son múltiples palabras, tomar las primeras 2-3 letras de la primera palabra
            var primeraPalabra = palabras[0];
            if (primeraPalabra.Length >= 3)
            {
                return primeraPalabra.Substring(0, 3);
            }
            return primeraPalabra;
        }
    }
}