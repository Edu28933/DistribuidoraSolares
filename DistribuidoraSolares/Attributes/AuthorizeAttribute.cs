using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DistribuidoraSolares.Attributes;

public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");
        
        if (usuarioId == null)
        {
            // Usuario no autenticado, redirigir al login
            context.Result = new RedirectToActionResult("Index", "Login", null);
        }
    }
}

public class AllowAnonymousAttribute : Attribute
{
    // Este atributo permite acceso sin autenticación
}
