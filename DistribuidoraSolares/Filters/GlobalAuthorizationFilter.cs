using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Filters;

public class GlobalAuthorizationFilter : IAuthorizationFilter
{
    private static readonly string[] PublicControllers = { "Login" };
    private static readonly string[] PublicActionsForProductos = { "Index" };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var action = context.RouteData.Values["action"]?.ToString() ?? "";

        // Permitir acceso público a Login controller
        if (PublicControllers.Contains(controller, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        // Verificar si el usuario está autenticado
        var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");
        
        if (usuarioId == null)
        {
            // Usuario no autenticado, redirigir al login
            context.Result = new RedirectToActionResult("Index", "Login", null);
            return;
        }

        // Obtener el rol del usuario
        var rolNombre = context.HttpContext.Session.GetString("RolNombre") ?? "";
        
        // SuperAdmin y Admin tienen acceso completo (sin verificar permisos)
        if (rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) || 
            rolNombre.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return; // Permitir acceso completo
        }

        // Para TODOS los demás roles: un permiso por módulo (Index). Ver/Crear/Editar/Eliminar según la acción.
        try
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var permisoPantallaService = serviceProvider.GetRequiredService<IPermisoPantallaService>();

            var tipoPermiso = action switch
            {
                "Create" => "Crear",
                "Edit" => "Editar",
                "Delete" => "Eliminar",
                _ => "Ver"
            };

            var tienePermiso = permisoPantallaService.TienePermisoAsync(
                usuarioId.Value,
                controller,
                action,
                tipoPermiso
            ).GetAwaiter().GetResult();

            if (!tienePermiso)
            {
                // Si es Usuario Nuevo sin permisos, redirigir a Productos
                if (rolNombre.Equals("Usuario Nuevo", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new RedirectToActionResult("Index", "Productos", null);
                }
                else
                {
                    context.Result = new RedirectToActionResult("Index", "Dashboard", null);
                }
                context.HttpContext.Items["Error"] = "No tiene permisos para acceder a esta sección.";
                return;
            }
        }
        catch (Exception ex)
        {
            // Si hay error al verificar permisos, denegar acceso por seguridad
            // Esto evita que usuarios accedan a áreas restringidas si hay problemas con la base de datos
            System.Diagnostics.Debug.WriteLine($"Error al verificar permisos: {ex.Message}");
            context.Result = new RedirectToActionResult("Index", "Dashboard", null);
            context.HttpContext.Items["Error"] = "Error al verificar permisos. Contacte al administrador.";
            return;
        }
    }
}
