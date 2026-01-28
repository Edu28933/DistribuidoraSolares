-- Script para asignar el rol SuperAdmin al usuario Edu289

-- Primero, verificar si existe el rol SuperAdmin y obtener su ID
DECLARE @RolSuperAdminId INT;
DECLARE @UsuarioId INT;

-- Buscar el rol SuperAdmin (puede estar como "SuperAdmin", "Super Administrador", etc.)
SELECT @RolSuperAdminId = RolId 
FROM tbl_roles 
WHERE UPPER(LTRIM(RTRIM(Nombre))) IN ('SUPERADMIN', 'SUPER ADMINISTRADOR', 'SUPER ADMIN')
  AND Estado = 'ACTIVO';

-- Si no existe el rol SuperAdmin, crearlo
IF @RolSuperAdminId IS NULL
BEGIN
    INSERT INTO tbl_roles (Nombre, Estado)
    VALUES ('SuperAdmin', 'ACTIVO');
    
    SET @RolSuperAdminId = SCOPE_IDENTITY();
    
    PRINT 'Rol SuperAdmin creado con ID: ' + CAST(@RolSuperAdminId AS VARCHAR(10));
END
ELSE
BEGIN
    PRINT 'Rol SuperAdmin encontrado con ID: ' + CAST(@RolSuperAdminId AS VARCHAR(10));
END

-- Buscar el usuario Edu289
SELECT @UsuarioId = UsuarioId 
FROM tbl_usuarios 
WHERE UsuarioLogin = 'Edu289'
  AND Estado = 'ACTIVO';

IF @UsuarioId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el usuario Edu289';
    RETURN;
END

PRINT 'Usuario Edu289 encontrado con ID: ' + CAST(@UsuarioId AS VARCHAR(10));

-- Verificar el hash actual antes de actualizar
DECLARE @HashActual NVARCHAR(MAX);
SELECT @HashActual = PasswordHash 
FROM tbl_usuarios 
WHERE UsuarioId = @UsuarioId;

PRINT 'Hash actual de contraseña: ' + ISNULL(@HashActual, 'NULL');
PRINT 'Hash esperado para "Edu*28933": RWR1KjI4OTMz';

-- Asegurar que el hash sea correcto (por si hay espacios o caracteres invisibles)
-- El hash Base64 de "Edu*28933" es: RWR1KjI4OTMz
DECLARE @PasswordHashCorrecto NVARCHAR(MAX) = 'RWR1KjI4OTMz';

-- Actualizar el rol y asegurar que el hash de contraseña sea correcto
UPDATE tbl_usuarios
SET RolId = @RolSuperAdminId,
    PasswordHash = @PasswordHashCorrecto
WHERE UsuarioId = @UsuarioId;

IF @@ROWCOUNT > 0
BEGIN
    PRINT 'Usuario Edu289 actualizado exitosamente al rol SuperAdmin (RolId: ' + CAST(@RolSuperAdminId AS VARCHAR(10)) + ')';
    PRINT '';
    PRINT '========================================';
    PRINT 'CREDENCIALES DE ACCESO:';
    PRINT 'Usuario: Edu289 (exactamente así, con mayúscula E y minúsculas)';
    PRINT 'Contraseña: Edu*28933 (exactamente así)';
    PRINT '========================================';
    PRINT '';
    PRINT 'IMPORTANTE:';
    PRINT '- El usuario debe escribirse exactamente: Edu289 (case-sensitive)';
    PRINT '- La contraseña debe escribirse exactamente: Edu*28933';
    PRINT '- Verifique que no haya espacios antes o después al escribir';
    PRINT '';
    
    -- Mostrar información del usuario actualizado
    SELECT 
        u.UsuarioId,
        u.UsuarioLogin,
        u.Nombre,
        r.RolId,
        r.Nombre AS RolNombre,
        u.Estado
    FROM tbl_usuarios u
    INNER JOIN tbl_roles r ON u.RolId = r.RolId
    WHERE u.UsuarioId = @UsuarioId;
END
ELSE
BEGIN
    PRINT 'ERROR: No se pudo actualizar el usuario';
END
GO
