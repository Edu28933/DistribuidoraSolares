-- =============================================
-- MODIFICAR STORED PROCEDURES DE CREACIÓN
-- Agregar parámetro @SucursalId para asignar sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_venta_completa
-- =============================================
-- IMPORTANTE: Este stored procedure es complejo y necesita revisión manual
-- Pasos a seguir:
-- 1. Agregar @SucursalId INT = NULL como parámetro
-- 2. Después de obtener @UsuarioId, calcular SucursalId:
--    DECLARE @SucursalIdCalculado INT;
--    IF @SucursalId IS NULL
--    BEGIN
--        SELECT @SucursalIdCalculado = SucursalId 
--        FROM tbl_usuarios 
--        WHERE UsuarioId = @UsuarioId;
--    END
--    ELSE
--    BEGIN
--        SET @SucursalIdCalculado = @SucursalId;
--    END
-- 3. En el INSERT INTO tbl_ventas, agregar SucursalId:
--    INSERT INTO tbl_ventas (..., SucursalId, ...) 
--    VALUES (..., @SucursalIdCalculado, ...)
-- 4. Al crear movimientos de inventario (INSERT INTO tbl_inventario_movimientos),
--    también usar @SucursalIdCalculado para la columna SucursalId

PRINT 'NOTA: usp_venta_completa requiere modificación manual';
PRINT 'Ver instrucciones arriba para agregar @SucursalId';
GO

-- =============================================
-- Modificar usp_compra_completa
-- =============================================
-- IMPORTANTE: Este stored procedure es complejo y necesita revisión manual
-- Pasos a seguir:
-- 1. Agregar @SucursalId INT = NULL como parámetro
-- 2. Después de obtener @UsuarioId, calcular SucursalId:
--    DECLARE @SucursalIdCalculado INT;
--    IF @SucursalId IS NULL
--    BEGIN
--        SELECT @SucursalIdCalculado = SucursalId 
--        FROM tbl_usuarios 
--        WHERE UsuarioId = @UsuarioId;
--    END
--    ELSE
--    BEGIN
--        SET @SucursalIdCalculado = @SucursalId;
--    END
-- 3. En el INSERT INTO tbl_compras, agregar SucursalId:
--    INSERT INTO tbl_compras (..., SucursalId, ...) 
--    VALUES (..., @SucursalIdCalculado, ...)
-- 4. Al crear movimientos de inventario (INSERT INTO tbl_inventario_movimientos),
--    también usar @SucursalIdCalculado para la columna SucursalId

PRINT 'NOTA: usp_compra_completa requiere modificación manual';
PRINT 'Ver instrucciones arriba para agregar @SucursalId';
GO

-- =============================================
-- Modificar usp_reserva_crear_completa
-- =============================================
-- IMPORTANTE: Este stored procedure es complejo y necesita revisión manual
-- Pasos a seguir:
-- 1. Agregar @SucursalId INT = NULL como parámetro
-- 2. Después de obtener @UsuarioId, calcular SucursalId:
--    DECLARE @SucursalIdCalculado INT;
--    IF @SucursalId IS NULL
--    BEGIN
--        SELECT @SucursalIdCalculado = SucursalId 
--        FROM tbl_usuarios 
--        WHERE UsuarioId = @UsuarioId;
--    END
--    ELSE
--    BEGIN
--        SET @SucursalIdCalculado = @SucursalId;
--    END
-- 3. En el INSERT INTO tbl_reservas, agregar SucursalId:
--    INSERT INTO tbl_reservas (..., SucursalId, ...) 
--    VALUES (..., @SucursalIdCalculado, ...)
-- 4. Al crear movimientos de inventario (INSERT INTO tbl_inventario_movimientos),
--    también usar @SucursalIdCalculado para la columna SucursalId

PRINT 'NOTA: usp_reserva_crear_completa requiere modificación manual';
PRINT 'Ver instrucciones arriba para agregar @SucursalId';
GO

-- =============================================
-- Modificar usp_caja_abrir
-- =============================================
-- IMPORTANTE: Este stored procedure necesita revisión manual
-- Pasos a seguir:
-- 1. Agregar @SucursalId INT = NULL como parámetro
-- 2. Obtener UsuarioId de la sesión o como parámetro adicional
--    Si ya recibe @UsuarioId, usarlo. Si no, necesitarás agregarlo también.
-- 3. Calcular SucursalId:
--    DECLARE @SucursalIdCalculado INT;
--    IF @SucursalId IS NULL
--    BEGIN
--        SELECT @SucursalIdCalculado = SucursalId 
--        FROM tbl_usuarios 
--        WHERE UsuarioId = @UsuarioId;
--    END
--    ELSE
--    BEGIN
--        SET @SucursalIdCalculado = @SucursalId;
--    END
-- 4. En el INSERT INTO tbl_caja, agregar SucursalId:
--    INSERT INTO tbl_caja (..., SucursalId, ...) 
--    VALUES (..., @SucursalIdCalculado, ...)

PRINT 'NOTA: usp_caja_abrir requiere modificación manual';
PRINT 'Ver instrucciones arriba para agregar @SucursalId';
PRINT 'IMPORTANTE: Verificar si usp_caja_abrir recibe @UsuarioId como parámetro';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE CREACIÓN';
PRINT '========================================';
PRINT 'usp_venta_completa: ⚠️ Requiere modificación manual';
PRINT 'usp_compra_completa: ⚠️ Requiere modificación manual';
PRINT 'usp_reserva_crear_completa: ⚠️ Requiere modificación manual';
PRINT 'usp_caja_abrir: ⚠️ Requiere modificación manual';
PRINT '========================================';
PRINT 'NOTA: Estos stored procedures son complejos y manejan transacciones.';
PRINT 'Se recomienda revisar cada uno manualmente siguiendo las instrucciones.';
PRINT '========================================';
GO
