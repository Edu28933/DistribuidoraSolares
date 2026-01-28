using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DistribuidoraSolares.Attributes;

public class AuthorizeRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _rolesPermitidos;

    public AuthorizeRoleAttribute(params string[] rolesPermitidos)
    {
        _rolesPermitidos = rolesPermitidos;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");
        
        if (usuarioId == null)
        {
            // Usuario no autenticado, redirigir al login
            context.Result = new RedirectToActionResult("Index", "Login", null);
            return;
        }

        var rolNombre = context.HttpContext.Session.GetString("RolNombre") ?? "";

        // Verificar si el rol del usuario está en la lista de roles permitidos
        if (!_rolesPermitidos.Any(r => r.Equals(rolNombre, StringComparison.OrdinalIgnoreCase)))
        {
            // Usuario no tiene permisos, mostrar error
            context.Result = new RedirectToActionResult("Index", "Dashboard", null);
            context.HttpContext.Items["Error"] = "No tiene permisos para acceder a esta sección";
        }
    }
}
