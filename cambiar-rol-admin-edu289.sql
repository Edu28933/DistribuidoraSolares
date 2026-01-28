-- Script para cambiar SOLO el rol de Edu289 a Administrador
-- NO modifica la contraseña ni ningún otro campo

DECLARE @RolAdminId INT;
DECLARE @UsuarioId INT;

-- Buscar el rol Administrador o Admin
SELECT @RolAdminId = RolId 
FROM tbl_roles 
WHERE UPPER(LTRIM(RTRIM(Nombre))) IN ('ADMIN', 'ADMINISTRADOR')
  AND Estado = 'ACTIVO';

IF @RolAdminId IS NULL
BEGIN
    PRINT 'ERROR: No se encontró el rol Administrador';
    RETURN;
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

-- Actualizar SOLO el RolId
UPDATE tbl_usuarios
SET RolId = @RolAdminId
WHERE UsuarioId = @UsuarioId;

PRINT 'Rol actualizado a Administrador';

SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    RolId,
    Estado
FROM tbl_usuarios
WHERE UsuarioId = @UsuarioId;

GO
