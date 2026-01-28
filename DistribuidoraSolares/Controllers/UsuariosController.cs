using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Models;
using DistribuidoraSolares.Services;

namespace DistribuidoraSolares.Controllers;

public class UsuariosController : Controller
{
    private readonly IUsuarioService _usuarioService;
    private readonly IRolService _rolService;
    private readonly IUsuarioEmailService _usuarioEmailService;
    private readonly ISucursalService _sucursalService;

    public UsuariosController(IUsuarioService usuarioService, IRolService rolService, IUsuarioEmailService usuarioEmailService, ISucursalService sucursalService)
    {
        _usuarioService = usuarioService;
        _rolService = rolService;
        _usuarioEmailService = usuarioEmailService;
        _sucursalService = sucursalService;
    }

    private bool EsSuperAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
    }

    private bool EsAdmin()
    {
        var rolNombre = HttpContext.Session.GetString("RolNombre") ?? "";
        return rolNombre.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    }

    private int? GetUsuarioId()
    {
        return HttpContext.Session.GetInt32("UsuarioId");
    }

    public async Task<IActionResult> Index(string? buscar)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        List<Usuario> usuarios;
        
        try
        {
            if (!string.IsNullOrEmpty(buscar))
            {
                usuarios = await _usuarioService.BuscarUsuariosAsync(buscar);
            }
            else
            {
                usuarios = await _usuarioService.MostrarUsuariosAsync();
            }

            // Filtrar solo usuarios activos
            usuarios = usuarios.Where(u => u.Estado == "ACTIVO").ToList();

            // Aplicar permisos según el rol
            if (EsSuperAdmin())
            {
                // SuperAdmin ve todos los usuarios activos
            }
            else if (EsAdmin())
            {
                // Admin solo ve usuarios de su sucursal
                var sucursalIdAdmin = HttpContext.Session.GetInt32("SucursalId");
                if (sucursalIdAdmin.HasValue)
                {
                    usuarios = usuarios.Where(u => u.SucursalId == sucursalIdAdmin.Value).ToList();
                }
                else
                {
                    usuarios = new List<Usuario>(); // Si Admin no tiene sucursal, no ve usuarios
                }
            }
            else
            {
                // Otros usuarios solo pueden ver su propio usuario
                usuarios = usuarios.Where(u => u.UsuarioId == usuarioIdActual).ToList();
            }
        }
        catch (Exception ex)
        {
            usuarios = new List<Usuario>();
            TempData["Error"] = $"Error al cargar usuarios: {ex.Message}";
        }

        ViewBag.Buscar = buscar;
        ViewBag.EsSuperAdmin = EsSuperAdmin();
        ViewBag.EsAdmin = EsAdmin();
        return View(usuarios);
    }

    public async Task<IActionResult> Create()
    {
        // Solo SuperAdmin puede crear usuarios
        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para crear usuarios";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var roles = await _rolService.MostrarRolesAsync();
            ViewBag.Roles = roles.Where(r => r.Estado == "ACTIVO").ToList();
            
            // Cargar sucursales activas
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar datos: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
        
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario usuario, string? password, string? email)
    {
        // Solo SuperAdmin puede crear usuarios
        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para crear usuarios";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Si se proporciona una contraseña, hashearla (simplificado - en producción usar BCrypt o similar)
                if (!string.IsNullOrEmpty(password))
                {
                    usuario.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
                }
                
                var usuarioId = await _usuarioService.CrearUsuarioAsync(usuario);
                
                // Guardar correo electrónico si se proporciona
                if (!string.IsNullOrEmpty(email) && usuarioId > 0)
                {
                    try
                    {
                        await _usuarioEmailService.CrearOActualizarEmailAsync(usuarioId, email);
                    }
                    catch (Exception ex)
                    {
                        // Log el error pero no fallar la creación del usuario
                        TempData["Warning"] = $"Usuario creado pero hubo un error al guardar el correo electrónico: {ex.Message}";
                    }
                }
                
                TempData["Success"] = "Usuario creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al crear el usuario: {ex.Message}";
            }
        }
        
        try
        {
            var roles = await _rolService.MostrarRolesAsync();
            ViewBag.Roles = roles.Where(r => r.Estado == "ACTIVO").ToList();
            
            // Cargar sucursales activas
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
        }
        catch { }
        
        return View(usuario);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == id && u.Estado == "ACTIVO");
            
            if (usuario == null)
            {
                return NotFound();
            }

            // Verificar permisos
            if (!EsSuperAdmin())
            {
                if (EsAdmin())
                {
                    // Admin solo puede ver usuarios, no editarlos completamente
                    // Pero puede modificar el rol (esto se manejará en la vista)
                    ViewBag.SoloModificarRol = true;
                }
                else
                {
                    // Otros usuarios solo pueden ver su propio usuario
                    if (usuario.UsuarioId != usuarioIdActual)
                    {
                        TempData["Error"] = "No tiene permisos para editar este usuario";
                        return RedirectToAction(nameof(Index));
                    }
                    ViewBag.SoloLectura = true;
                }
            }

            // Obtener correo electrónico del usuario
            var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(id);
            ViewBag.Email = usuarioEmail?.Email ?? "";

            var roles = await _rolService.MostrarRolesAsync();
            ViewBag.Roles = roles.Where(r => r.Estado == "ACTIVO").ToList();
            
            // Cargar sucursales activas
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            
            ViewBag.EsSuperAdmin = EsSuperAdmin();
            ViewBag.EsAdmin = EsAdmin();
            
            return View(usuario);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el usuario: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Usuario usuario, string? password, string? email)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (id != usuario.UsuarioId)
        {
            return NotFound();
        }

        // Verificar permisos
        if (!EsSuperAdmin())
        {
            if (EsAdmin())
            {
                // Admin solo puede modificar el rol de usuarios de su sucursal, no otros campos
                var usuarios = await _usuarioService.MostrarUsuariosAsync();
                var usuarioActual = usuarios.FirstOrDefault(u => u.UsuarioId == id);
                
                if (usuarioActual == null)
                {
                    TempData["Error"] = "Usuario no encontrado";
                    return RedirectToAction(nameof(Index));
                }
                
                // Verificar que el usuario pertenece a la sucursal del Admin
                var sucursalIdAdmin = HttpContext.Session.GetInt32("SucursalId");
                if (!sucursalIdAdmin.HasValue || usuarioActual.SucursalId != sucursalIdAdmin.Value)
                {
                    TempData["Error"] = "Solo puede modificar usuarios de su sucursal";
                    return RedirectToAction(nameof(Index));
                }
                
                // Mantener todos los campos excepto el RolId (y SucursalId)
                usuario.Nombre = usuarioActual.Nombre;
                usuario.UsuarioLogin = usuarioActual.UsuarioLogin;
                usuario.PasswordHash = usuarioActual.PasswordHash;
                usuario.Estado = usuarioActual.Estado;
                usuario.SucursalId = usuarioActual.SucursalId; // No puede cambiar la sucursal
            }
            else
            {
                // Otros usuarios solo pueden modificar su propio usuario (solo contraseña)
                if (usuario.UsuarioId != usuarioIdActual)
                {
                    TempData["Error"] = "Solo puede modificar su propio usuario";
                    return RedirectToAction(nameof(Index));
                }
                
                // Mantener todos los campos excepto la contraseña (si se proporciona)
                var usuarios = await _usuarioService.MostrarUsuariosAsync();
                var usuarioActual = usuarios.FirstOrDefault(u => u.UsuarioId == id);
                if (usuarioActual != null)
                {
                    usuario.Nombre = usuarioActual.Nombre;
                    usuario.UsuarioLogin = usuarioActual.UsuarioLogin;
                    usuario.RolId = usuarioActual.RolId;
                    usuario.Estado = usuarioActual.Estado;
                    usuario.SucursalId = usuarioActual.SucursalId;
                }
            }
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Si se proporciona una nueva contraseña, hashearla
                if (string.IsNullOrEmpty(password))
                {
                    // Mantener la contraseña actual si no se proporciona una nueva
                    var usuarios = await _usuarioService.MostrarUsuariosAsync();
                    var usuarioActual = usuarios.FirstOrDefault(u => u.UsuarioId == id);
                    if (usuarioActual != null)
                    {
                        usuario.PasswordHash = usuarioActual.PasswordHash;
                    }
                }
                else
                {
                    // Solo SuperAdmin puede cambiar contraseñas
                    if (!EsSuperAdmin())
                    {
                        TempData["Error"] = "No tiene permisos para cambiar contraseñas";
                        return RedirectToAction(nameof(Edit), new { id });
                    }
                    usuario.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
                }
                
                await _usuarioService.EditarUsuarioAsync(usuario);
                
                // Actualizar correo electrónico si se proporciona y es SuperAdmin
                if (!string.IsNullOrEmpty(email) && EsSuperAdmin())
                {
                    try
                    {
                        await _usuarioEmailService.CrearOActualizarEmailAsync(id, email);
                    }
                    catch (Exception ex)
                    {
                        TempData["Warning"] = $"Usuario actualizado pero hubo un error al guardar el correo electrónico: {ex.Message}";
                    }
                }
                
                TempData["Success"] = "Usuario actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al actualizar el usuario: {ex.Message}";
            }
        }

        try
        {
            var roles = await _rolService.MostrarRolesAsync();
            ViewBag.Roles = roles.Where(r => r.Estado == "ACTIVO").ToList();
            
            // Cargar sucursales activas
            var sucursales = await _sucursalService.MostrarSucursalesAsync();
            ViewBag.Sucursales = sucursales.Where(s => s.Estado == "ACTIVO").ToList();
            
            ViewBag.EsSuperAdmin = EsSuperAdmin();
            ViewBag.EsAdmin = EsAdmin();
        }
        catch { }

        return View(usuario);
    }

    public async Task<IActionResult> Perfil()
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == usuarioIdActual && u.Estado == "ACTIVO");
            
            if (usuario == null)
            {
                return NotFound();
            }

            // Obtener correo electrónico del usuario
            var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
            ViewBag.Email = usuarioEmail?.Email ?? "";

            return View(usuario);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar el perfil: {ex.Message}";
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(string? passwordActual, string? passwordNueva, string? passwordConfirmar, string? email)
    {
        var usuarioIdActual = GetUsuarioId();
        if (usuarioIdActual == null)
        {
            return RedirectToAction("Index", "Login");
        }

        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == usuarioIdActual && u.Estado == "ACTIVO");
            
            if (usuario == null)
            {
                return NotFound();
            }

            // Validar contraseña actual si se quiere cambiar la contraseña
            if (!string.IsNullOrEmpty(passwordNueva))
            {
                if (string.IsNullOrEmpty(passwordActual))
                {
                    TempData["Error"] = "Debe ingresar su contraseña actual para cambiarla";
                    var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
                    ViewBag.Email = usuarioEmail?.Email ?? "";
                    return View(usuario);
                }

                // Verificar contraseña actual
                var passwordHashActual = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordActual));
                if (usuario.PasswordHash != passwordHashActual)
                {
                    TempData["Error"] = "La contraseña actual es incorrecta";
                    var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
                    ViewBag.Email = usuarioEmail?.Email ?? "";
                    return View(usuario);
                }

                // Validar que las nuevas contraseñas coincidan
                if (passwordNueva != passwordConfirmar)
                {
                    TempData["Error"] = "Las nuevas contraseñas no coinciden";
                    var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
                    ViewBag.Email = usuarioEmail?.Email ?? "";
                    return View(usuario);
                }

                if (passwordNueva.Length < 4)
                {
                    TempData["Error"] = "La nueva contraseña debe tener al menos 4 caracteres";
                    var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
                    ViewBag.Email = usuarioEmail?.Email ?? "";
                    return View(usuario);
                }

                // Actualizar contraseña
                usuario.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordNueva));
            }

            // Actualizar usuario
            await _usuarioService.EditarUsuarioAsync(usuario);

            // Actualizar correo electrónico si se proporciona
            if (!string.IsNullOrEmpty(email))
            {
                try
                {
                    await _usuarioEmailService.CrearOActualizarEmailAsync(usuarioIdActual.Value, email);
                }
                catch (Exception ex)
                {
                    TempData["Warning"] = $"Perfil actualizado pero hubo un error al guardar el correo electrónico: {ex.Message}";
                }
            }

            TempData["Success"] = "Perfil actualizado exitosamente";
            return RedirectToAction(nameof(Perfil));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al actualizar el perfil: {ex.Message}";
            var usuarioEmail = await _usuarioEmailService.ObtenerEmailPorUsuarioAsync(usuarioIdActual.Value);
            ViewBag.Email = usuarioEmail?.Email ?? "";
            
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == usuarioIdActual && u.Estado == "ACTIVO");
            return View(usuario ?? new Usuario());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Solo SuperAdmin puede eliminar usuarios
        if (!EsSuperAdmin())
        {
            TempData["Error"] = "No tiene permisos para eliminar usuarios";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _usuarioService.EliminarUsuarioAsync(id);
            TempData["Success"] = "Usuario eliminado exitosamente";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el usuario: {ex.Message}";
        }
        
        return RedirectToAction(nameof(Index));
    }
}
