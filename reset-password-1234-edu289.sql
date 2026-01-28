-- Script RÁPIDO para resetear contraseña de Edu289 a "1234"
-- Hash Base64 de "1234" = MTIzNA==

UPDATE tbl_usuarios
SET PasswordHash = 'MTIzNA=='
WHERE UsuarioLogin = 'Edu289' AND Estado = 'ACTIVO';

SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    PasswordHash,
    Estado,
    RolId
FROM tbl_usuarios
WHERE UsuarioLogin = 'Edu289';

PRINT '========================================';
PRINT 'CONTRASEÑA RESETEADA';
PRINT 'Usuario: Edu289';
PRINT 'Contraseña: 1234';
PRINT '========================================';

GO
