using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using DistribuidoraSolares.Data;
using DistribuidoraSolares.Models;
using System.Data;

namespace DistribuidoraSolares.Services;

public interface IAuthService
{
    Task<Usuario?> ValidarCredencialesAsync(string usuarioLogin, string password);
    Task<int> ObtenerRolIdUsuarioNuevoAsync();
    Task<bool> ExisteUsuarioLoginAsync(string usuarioLogin);
    Task<string?> RecuperarPasswordAsync(string email);
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsuarioService _usuarioService;
    private readonly IRolService _rolService;

    public AuthService(ApplicationDbContext context, IUsuarioService usuarioService, IRolService rolService)
    {
        _context = context;
        _usuarioService = usuarioService;
        _rolService = rolService;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string usuarioLogin, string password)
    {
        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            
            // DEBUG: Ver qué usuarios se están obteniendo
            System.Diagnostics.Debug.WriteLine($"=== DEBUG AUTENTICACIÓN ===");
            System.Diagnostics.Debug.WriteLine($"Usuario buscado: '{usuarioLogin}'");
            System.Diagnostics.Debug.WriteLine($"Total usuarios obtenidos: {usuarios.Count}");
            
            foreach (var u in usuarios)
            {
                System.Diagnostics.Debug.WriteLine($"  - Login: '{u.UsuarioLogin}', Estado: '{u.Estado}', Hash: '{u.PasswordHash}'");
            }
            
            // Comparación case-sensitive (reconoce mayúsculas y minúsculas)
            var usuario = usuarios.FirstOrDefault(u => 
                u.UsuarioLogin.Equals(usuarioLogin, StringComparison.Ordinal) && 
                u.Estado == "ACTIVO");

            if (usuario == null)
            {
                System.Diagnostics.Debug.WriteLine($"Usuario NO encontrado o inactivo");
                return null;
            }

            System.Diagnostics.Debug.WriteLine($"Usuario encontrado: '{usuario.UsuarioLogin}'");
            System.Diagnostics.Debug.WriteLine($"Hash almacenado: '{usuario.PasswordHash}' (longitud: {usuario.PasswordHash?.Length ?? 0})");

            // Hash de la contraseña ingresada (mismo método que se usa al crear)
            var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
            
            System.Diagnostics.Debug.WriteLine($"Contraseña ingresada: '{password}'");
            System.Diagnostics.Debug.WriteLine($"Hash calculado: '{passwordHash}' (longitud: {passwordHash.Length})");
            System.Diagnostics.Debug.WriteLine($"¿Coinciden?: {usuario.PasswordHash == passwordHash}");

            if (usuario.PasswordHash == passwordHash)
            {
                System.Diagnostics.Debug.WriteLine($"AUTENTICACIÓN EXITOSA");
                // Cargar el rol del usuario
                var roles = await _rolService.MostrarRolesAsync();
                usuario.Rol = roles.FirstOrDefault(r => r.RolId == usuario.RolId);
                return usuario;
            }

            System.Diagnostics.Debug.WriteLine($"AUTENTICACIÓN FALLIDA - Hash no coincide");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ERROR en ValidarCredencialesAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    public async Task<int> ObtenerRolIdUsuarioNuevoAsync()
    {
        try
        {
            var roles = await _rolService.MostrarRolesAsync();
            var rolUsuarioNuevo = roles.FirstOrDefault(r => 
                r.Nombre.Equals("Usuario Nuevo", StringComparison.OrdinalIgnoreCase));
            
            if (rolUsuarioNuevo == null)
            {
                throw new Exception("El rol 'Usuario Nuevo' no existe en la base de datos");
            }

            return rolUsuarioNuevo.RolId;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al obtener el rol 'Usuario Nuevo': {ex.Message}");
        }
    }

    public async Task<bool> ExisteUsuarioLoginAsync(string usuarioLogin)
    {
        try
        {
            var usuarios = await _usuarioService.MostrarUsuariosAsync();
            // Comparación case-sensitive (reconoce mayúsculas y minúsculas)
            return usuarios.Any(u => u.UsuarioLogin.Equals(usuarioLogin, StringComparison.Ordinal));
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> RecuperarPasswordAsync(string email)
    {
        try
        {
            var connectionString = _context.Database.GetConnectionString();
            
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("usp_usuario_recuperar_password", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Email", email));

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                var nuevaPassword = reader.GetString(reader.GetOrdinal("NuevaPassword"));
                var usuarioId = reader.GetInt32(reader.GetOrdinal("UsuarioId"));
                
                // Calcular el hash de la contraseña (igual que al crear usuario)
                var passwordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(nuevaPassword));
                
                // Actualizar la contraseña en la base de datos
                var usuarios = await _usuarioService.MostrarUsuariosAsync();
                var usuario = usuarios.FirstOrDefault(u => u.UsuarioId == usuarioId);
                if (usuario != null)
                {
                    usuario.PasswordHash = passwordHash;
                    await _usuarioService.EditarUsuarioAsync(usuario);
                }
                
                return nuevaPassword;
            }

            return null;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al recuperar contraseña: {ex.Message}");
        }
    }
}
