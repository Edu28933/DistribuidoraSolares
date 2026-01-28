-- Script para resetear la contraseña de Edu289 a "admin123" y asignar SuperAdmin

DECLARE @RolSuperAdminId INT;
DECLARE @UsuarioId INT;

-- Buscar o crear rol SuperAdmin
SELECT @RolSuperAdminId = RolId 
FROM tbl_roles 
WHERE UPPER(LTRIM(RTRIM(Nombre))) IN ('SUPERADMIN', 'SUPER ADMINISTRADOR', 'SUPER ADMIN')
  AND Estado = 'ACTIVO';

IF @RolSuperAdminId IS NULL
BEGIN
    INSERT INTO tbl_roles (Nombre, Estado) VALUES ('SuperAdmin', 'ACTIVO');
    SET @RolSuperAdminId = SCOPE_IDENTITY();
END

-- Buscar usuario Edu289
SELECT @UsuarioId = UsuarioId 
FROM tbl_usuarios 
WHERE LTRIM(RTRIM(UsuarioLogin)) = 'Edu289'
  AND Estado = 'ACTIVO';

IF @UsuarioId IS NULL
BEGIN
    PRINT 'ERROR: Usuario no encontrado';
    RETURN;
END

-- Hash Base64 de "admin123" = YWRtaW4xMjM=
-- Actualizar usuario con contraseña simple y rol SuperAdmin
UPDATE tbl_usuarios
SET RolId = @RolSuperAdminId,
    PasswordHash = 'YWRtaW4xMjM=',
    UsuarioLogin = 'Edu289'
WHERE UsuarioId = @UsuarioId;

PRINT '========================================';
PRINT 'USUARIO ACTUALIZADO';
PRINT '========================================';
PRINT 'Usuario: Edu289';
PRINT 'Contraseña: admin123';
PRINT 'Rol: SuperAdmin';
PRINT '========================================';

SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    RolId,
    Estado
FROM tbl_usuarios
WHERE UsuarioId = @UsuarioId;

GO
