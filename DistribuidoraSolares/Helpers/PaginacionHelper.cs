namespace DistribuidoraSolares.Helpers;

public class PaginacionHelper
{
    public static int ObtenerPaginaActual(int? pagina)
    {
        return pagina.HasValue && pagina.Value > 0 ? pagina.Value : 1;
    }

    public static int ObtenerTamañoPagina(int? tamañoPagina, int defaultTamaño = 20)
    {
        if (tamañoPagina.HasValue && tamañoPagina.Value > 0)
        {
            // Limitar el tamaño máximo a 100 para evitar problemas de rendimiento
            return tamañoPagina.Value > 100 ? 100 : tamañoPagina.Value;
        }
        return defaultTamaño;
    }

    public static (int skip, int take) CalcularSkipTake(int pagina, int tamañoPagina)
    {
        var skip = (pagina - 1) * tamañoPagina;
        return (skip, tamañoPagina);
    }
}

public class PaginacionResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PaginaActual { get; set; }
    public int TamañoPagina { get; set; }
    public int TotalItems { get; set; }
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItems / TamañoPagina);
    public bool TienePaginaAnterior => PaginaActual > 1;
    public bool TienePaginaSiguiente => PaginaActual < TotalPaginas;
}
