-- Script para asignar rol SuperAdmin a Edu289
-- SuperAdmin tiene acceso completo: crear, editar, eliminar usuarios y modificar todo

DECLARE @RolSuperAdminId INT;
DECLARE @UsuarioId INT;

-- Buscar o crear el rol SuperAdmin
SELECT @RolSuperAdminId = RolId 
FROM tbl_roles 
WHERE UPPER(LTRIM(RTRIM(Nombre))) IN ('SUPERADMIN', 'SUPER ADMINISTRADOR', 'SUPER ADMIN')
  AND Estado = 'ACTIVO';

-- Si no existe, crearlo
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

-- Buscar usuario Edu289
SELECT @UsuarioId = UsuarioId 
FROM tbl_usuarios 
WHERE UsuarioLogin = 'Edu289'
  AND Estado = 'ACTIVO';

IF @UsuarioId IS NULL
BEGIN
    PRINT 'ERROR: Usuario Edu289 no encontrado';
    RETURN;
END

-- Actualizar el rol a SuperAdmin
UPDATE tbl_usuarios
SET RolId = @RolSuperAdminId
WHERE UsuarioId = @UsuarioId;

IF @@ROWCOUNT > 0
BEGIN
    PRINT '';
    PRINT '========================================';
    PRINT 'USUARIO ACTUALIZADO A SUPERADMIN';
    PRINT '========================================';
    PRINT 'Usuario: Edu289';
    PRINT 'Rol: SuperAdmin (ID: ' + CAST(@RolSuperAdminId AS VARCHAR(10)) + ')';
    PRINT '';
    PRINT 'PERMISOS DE SUPERADMIN:';
    PRINT '- Crear usuarios';
    PRINT '- Editar usuarios (todos los campos)';
    PRINT '- Eliminar usuarios';
    PRINT '- Cambiar contraseñas';
    PRINT '- Modificar correos electrónicos';
    PRINT '- Acceso completo a todas las funcionalidades';
    PRINT '========================================';
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
