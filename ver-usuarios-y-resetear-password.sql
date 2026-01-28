-- Script para VER todos los usuarios y sus contraseñas (hashes)
-- Esto te ayudará a identificar con qué usuario puedes entrar

SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    PasswordHash,
    -- Decodificar el hash para ver la contraseña (si es Base64 simple)
    -- Nota: Esto solo funciona si el hash es Base64 directo
    Estado,
    RolId,
    r.Nombre AS RolNombre
FROM tbl_usuarios u
LEFT JOIN tbl_roles r ON u.RolId = r.RolId
WHERE Estado = 'ACTIVO'
ORDER BY UsuarioId;

-- Script para establecer una contraseña SIMPLE y FUNCIONAR
-- Contraseña: 1234 (hash Base64: MTIzNA==)
-- Esto funcionará SEGURO

DECLARE @UsuarioId INT = 1; -- Cambia esto al UsuarioId que quieras

UPDATE tbl_usuarios
SET PasswordHash = 'MTIzNA=='
WHERE UsuarioId = @UsuarioId;

SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    PasswordHash,
    Estado,
    RolId
FROM tbl_usuarios
WHERE UsuarioId = @UsuarioId;

PRINT '========================================';
PRINT 'CONTRASEÑA ESTABLECIDA';
PRINT 'Usuario ID: ' + CAST(@UsuarioId AS VARCHAR(10));
PRINT 'Contraseña: 1234';
PRINT '========================================';

GO
