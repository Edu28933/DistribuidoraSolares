-- Script para VERIFICAR y CORREGIR el usuario Edu289
-- Este script muestra TODO y luego establece una contraseña que FUNCIONA

-- 1. Ver el estado actual
SELECT 
    UsuarioId,
    UsuarioLogin,
    LEN(UsuarioLogin) AS LongitudLogin,
    PasswordHash,
    LEN(PasswordHash) AS LongitudHash,
    Estado,
    RolId
FROM tbl_usuarios
WHERE UsuarioLogin = 'Edu289';

-- 2. Verificar que el hash sea exactamente "MTIzNA==" (para contraseña "1234")
-- Si no coincide, lo corregimos
DECLARE @UsuarioId INT;
SELECT @UsuarioId = UsuarioId FROM tbl_usuarios WHERE UsuarioLogin = 'Edu289' AND Estado = 'ACTIVO';

IF @UsuarioId IS NOT NULL
BEGIN
    -- Establecer contraseña "1234" (hash: MTIzNA==)
    UPDATE tbl_usuarios
    SET PasswordHash = 'MTIzNA=='
    WHERE UsuarioId = @UsuarioId;
    
    PRINT 'Contraseña establecida a: 1234';
    PRINT 'Hash: MTIzNA==';
    
    -- Verificar que quedó bien
    SELECT 
        UsuarioId,
        UsuarioLogin,
        PasswordHash,
        CASE WHEN PasswordHash = 'MTIzNA==' THEN 'CORRECTO' ELSE 'ERROR' END AS Verificacion,
        Estado
    FROM tbl_usuarios
    WHERE UsuarioId = @UsuarioId;
END
ELSE
BEGIN
    PRINT 'ERROR: Usuario Edu289 no encontrado';
END

GO
