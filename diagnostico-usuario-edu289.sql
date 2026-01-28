-- Script de diagnóstico para verificar el usuario Edu289 y su autenticación

-- Verificar el usuario y su hash
SELECT 
    UsuarioId,
    UsuarioLogin,
    Nombre,
    PasswordHash,
    LEN(PasswordHash) AS LongitudHash,
    Estado,
    RolId
FROM tbl_usuarios
WHERE UsuarioLogin = 'Edu289';

-- Verificar el rol actual
SELECT 
    r.RolId,
    r.Nombre AS RolNombre,
    r.Estado AS RolEstado
FROM tbl_roles r
INNER JOIN tbl_usuarios u ON r.RolId = u.RolId
WHERE u.UsuarioLogin = 'Edu289';

-- Calcular el hash Base64 de la contraseña "Edu*28933" para comparar
-- Nota: En SQL Server no hay una función directa para Base64, pero podemos verificar manualmente
-- El hash debería ser: RWR1KjI4OTMz

-- Verificar si el hash coincide exactamente
SELECT 
    UsuarioLogin,
    PasswordHash,
    CASE 
        WHEN PasswordHash = 'RWR1KjI4OTMz' THEN 'El hash coincide con Edu*28933'
        ELSE 'El hash NO coincide'
    END AS VerificacionHash,
    CASE 
        WHEN LEN(PasswordHash) = LEN('RWR1KjI4OTMz') THEN 'Longitud correcta'
        ELSE 'Longitud diferente'
    END AS VerificacionLongitud
FROM tbl_usuarios
WHERE UsuarioLogin = 'Edu289';

GO
