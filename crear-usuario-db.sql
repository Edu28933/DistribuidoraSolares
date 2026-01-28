-- Script para crear el usuario en la base de datos DistribuidoraSolares
-- Ejecuta este script conectándote a la base de datos DistribuidoraSolares

-- 1. Verificar si el usuario existe en la base de datos
SELECT name, type_desc 
FROM sys.database_principals 
WHERE name = 'Edu28933';

-- 2. Si el usuario NO existe, créalo con este comando:
-- CREATE USER [Edu28933] WITH PASSWORD = 'Edu*28933';

-- 3. Dar permisos al usuario (ejecuta después de crear el usuario)
-- ALTER ROLE db_datareader ADD MEMBER [Edu28933];
-- ALTER ROLE db_datawriter ADD MEMBER [Edu28933];
-- ALTER ROLE db_ddladmin ADD MEMBER [Edu28933];
-- GRANT EXECUTE ON SCHEMA::dbo TO [Edu28933];

-- NOTA: Si el usuario ya existe pero no tiene permisos, solo ejecuta los comandos GRANT y ALTER ROLE