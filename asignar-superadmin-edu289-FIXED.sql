-- Script CORREGIDO para asignar el rol SuperAdmin al usuario Edu289
-- Este script limpia espacios y asegura que todo esté correcto

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

-- Buscar el usuario Edu289 (sin importar mayúsculas/minúsculas en la búsqueda)
SELECT @UsuarioId = UsuarioId 
FROM tbl_usuarios 
WHERE LTRIM(RTRIM(UsuarioLogin)) = 'Edu289'
  AND Estado = 'ACTIVO';

IF @UsuarioId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el usuario Edu289';
    RETURN;
END

PRINT 'Usuario Edu289 encontrado con ID: ' + CAST(@UsuarioId AS VARCHAR(10));

-- Limpiar y normalizar el hash de contraseña
-- El hash Base64 de "Edu*28933" es: RWR1KjI4OTMz
DECLARE @PasswordHashCorrecto NVARCHAR(MAX) = 'RWR1KjI4OTMz';

-- Primero, limpiar cualquier espacio en blanco del hash actual
UPDATE tbl_usuarios
SET PasswordHash = LTRIM(RTRIM(PasswordHash))
WHERE UsuarioId = @UsuarioId;

-- Ahora actualizar el rol y asegurar que el hash sea exactamente el correcto
UPDATE tbl_usuarios
SET RolId = @RolSuperAdminId,
    PasswordHash = @PasswordHashCorrecto,
    UsuarioLogin = LTRIM(RTRIM(UsuarioLogin)) -- También limpiar el login por si acaso
WHERE UsuarioId = @UsuarioId;

IF @@ROWCOUNT > 0
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT 'USUARIO ACTUALIZADO EXITOSAMENTE';
    PRINT '========================================';
    PRINT 'Usuario: Edu289';
    PRINT 'Contraseña: Edu*28933';
    PRINT 'Rol: SuperAdmin (ID: ' + CAST(@RolSuperAdminId AS VARCHAR(10)) + ')';
    PRINT '========================================';
    PRINT '';
    
    -- Verificar que todo quedó correcto
    SELECT 
        u.UsuarioId,
        LTRIM(RTRIM(u.UsuarioLogin)) AS UsuarioLogin,
        u.Nombre,
        r.RolId,
        r.Nombre AS RolNombre,
        LTRIM(RTRIM(u.PasswordHash)) AS PasswordHash,
        CASE 
            WHEN LTRIM(RTRIM(u.PasswordHash)) = 'RWR1KjI4OTMz' THEN 'CORRECTO'
            ELSE 'ERROR - Hash no coincide'
        END AS VerificacionHash,
        u.Estado
    FROM tbl_usuarios u
    INNER JOIN tbl_roles r ON u.RolId = r.RolId
    WHERE u.UsuarioId = @UsuarioId;
    
    PRINT '';
    PRINT 'INSTRUCCIONES PARA INICIAR SESIÓN:';
    PRINT '1. Usuario: Edu289 (exactamente así)';
    PRINT '2. Contraseña: Edu*28933 (exactamente así)';
    PRINT '3. Si aún no funciona, reinicia la aplicación web';
END
ELSE
BEGIN
    PRINT 'ERROR: No se pudo actualizar el usuario';
END
GO
