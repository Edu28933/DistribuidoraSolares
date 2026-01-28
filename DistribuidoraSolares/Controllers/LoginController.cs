using Microsoft.AspNetCore.Mvc;
using DistribuidoraSolares.Services;
using DistribuidoraSolares.Models;
using System.Text;

namespace DistribuidoraSolares.Controllers;

public class LoginController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUsuarioService _usuarioService;
    private readonly IUsuarioEmailService _usuarioEmailService;
    private readonly ISucursalService _sucursalService;

    public LoginController(IAuthService authService, IUsuarioService usuarioService, IUsuarioEmailService usuarioEmailService, ISucursalService sucursalService)
    {
        _authService = authService;
        _usuarioService = usuarioService;
        _usuarioEmailService = usuarioEmailService;
        _sucursalService = sucursalService;
    }

    public IActionResult Index()
    {
        // Si ya está autenticado, redirigir al dashboard
        if (HttpContext.Session.GetInt32("UsuarioId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string usuarioLogin, string password)
    {
        if (string.IsNullOrEmpty(usuarioLogin) || string.IsNullOrEmpty(password))
        {
            TempData["Error"] = "Debe ingresar usuario y contraseña";
            ViewBag.UsuarioLogin = usuarioLogin; // Mantener el usuario ingresado
            return View("Index");
        }

        try
        {
            var usuario = await _authService.ValidarCredencialesAsync(usuarioLogin, password);

            if (usuario == null)
            {
                TempData["Error"] = "Usuario o contraseña incorrectos";
                ViewBag.UsuarioLogin = usuarioLogin; // Mantener el usuario ingresado, solo se borra la contraseña
                return View("Index");
            }

            // Guardar información del usuario en la sesión
            HttpContext.Session.SetInt32("UsuarioId", usuario.UsuarioId);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioLogin", usuario.UsuarioLogin);
            HttpContext.Session.SetInt32("RolId", usuario.RolId);
            HttpContext.Session.SetString("RolNombre", usuario.Rol?.Nombre ?? "");
            
            // Guardar información de sucursal
            if (usuario.SucursalId.HasValue)
            {
                HttpContext.Session.SetInt32("SucursalId", usuario.SucursalId.Value);
                // Cargar nombre de la sucursal
                var sucursal = await _sucursalService.ObtenerSucursalPorIdAsync(usuario.SucursalId.Value);
                HttpContext.Session.SetString("SucursalNombre", sucursal?.Nombre ?? "Sin sucursal");
            }
            else
            {
                // SuperAdmin/Contador no tienen sucursal asignada (ven todo)
                HttpContext.Session.SetInt32("SucursalId", 0);
                HttpContext.Session.SetString("SucursalNombre", "Todas las sucursales");
            }

            TempData["Success"] = $"Bienvenido, {usuario.Nombre}";
            
            // Si es Usuario Nuevo, redirigir a Productos; de lo contrario, al Dashboard
            var esUsuarioNuevo = usuario.Rol?.Nombre?.Equals("Usuario Nuevo", StringComparison.OrdinalIgnoreCase) ?? false;
            if (esUsuarioNuevo)
            {
                return RedirectToAction("Index", "Productos");
            }
            
            return RedirectToAction("Index", "Dashboard");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al iniciar sesión: {ex.Message}";
            ViewBag.UsuarioLogin = usuarioLogin; // Mantener el usuario ingresado
            return View("Index");
        }
    }

    public IActionResult Registro()
    {
        // Si ya está autenticado, redirigir al dashboard
        if (HttpContext.Session.GetInt32("UsuarioId") != null)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(string nombre, string usuarioLogin, string password, string confirmPassword, string email)
    {
        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(usuarioLogin) || 
            string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Todos los campos son obligatorios";
            ViewBag.Nombre = nombre;
            ViewBag.UsuarioLogin = usuarioLogin;
            ViewBag.Email = email;
            return View();
        }

        if (password != confirmPassword)
        {
            TempData["Error"] = "Las contraseñas no coinciden";
            // Mantener nombre y usuario, pero NO las contraseñas
            ViewBag.Nombre = nombre;
            ViewBag.UsuarioLogin = usuarioLogin;
            ViewBag.Email = email;
            return View();
        }

        if (password.Length < 4)
        {
            TempData["Error"] = "La contraseña debe tener al menos 4 caracteres";
            ViewBag.Nombre = nombre;
            ViewBag.UsuarioLogin = usuarioLogin;
            ViewBag.Email = email;
            return View();
        }

        try
        {
            // Verificar si el usuario ya existe
            var existeUsuario = await _authService.ExisteUsuarioLoginAsync(usuarioLogin);
            if (existeUsuario)
            {
                TempData["Error"] = "El nombre de usuario ya está en uso";
                ViewBag.Nombre = nombre;
                ViewBag.UsuarioLogin = usuarioLogin;
                ViewBag.Email = email;
                return View();
            }

            // Obtener el RolId de "Usuario Nuevo"
            var rolIdUsuarioNuevo = await _authService.ObtenerRolIdUsuarioNuevoAsync();

            // Crear el nuevo usuario
            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                UsuarioLogin = usuarioLogin,
                PasswordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(password)),
                RolId = rolIdUsuarioNuevo,
                Estado = "ACTIVO"
            };

            var usuarioId = await _usuarioService.CrearUsuarioAsync(nuevoUsuario);

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

            TempData["Success"] = "Usuario registrado exitosamente. Por favor, inicie sesión.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al registrar usuario: {ex.Message}";
            ViewBag.Nombre = nombre;
            ViewBag.UsuarioLogin = usuarioLogin;
            ViewBag.Email = email;
            return View();
        }
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["Success"] = "Sesión cerrada exitosamente";
        return RedirectToAction("Index");
    }

    public IActionResult RecuperarPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Error"] = "Debe ingresar un correo electrónico";
            return View();
        }

        try
        {
            var nuevaPassword = await _authService.RecuperarPasswordAsync(email);
            
            if (string.IsNullOrEmpty(nuevaPassword))
            {
                TempData["Error"] = "No se encontró un usuario activo con ese correo electrónico";
                return View();
            }

            // En producción, aquí deberías enviar el email con la nueva contraseña
            // Por ahora, solo mostramos un mensaje (NO es seguro en producción)
            TempData["Success"] = $"Se ha generado una nueva contraseña temporal. Su nueva contraseña es: {nuevaPassword}. Por favor, cámbiela después de iniciar sesión.";
            TempData["Info"] = "NOTA: En producción, esta contraseña se enviaría por correo electrónico. Por seguridad, cámbiela después de iniciar sesión.";
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al recuperar contraseña: {ex.Message}";
            return View();
        }
    }
}
