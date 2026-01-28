-- =============================================
-- SCRIPT DE MIGRACIÓN: IMPLEMENTACIÓN MULTI-SUCURSAL
-- Sistema Distribuidora Solares
-- =============================================
-- IMPORTANTE: HACER BACKUP DE LA BASE DE DATOS ANTES DE EJECUTAR ESTE SCRIPT
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- PASO 1: Crear tabla de Sucursales
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbl_sucursales]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbl_sucursales](
        [SucursalId] [int] IDENTITY(1,1) NOT NULL,
        [Nombre] [varchar](150) NOT NULL,
        [Codigo] [varchar](20) NULL,
        [Direccion] [varchar](255) NULL,
        [Telefono] [varchar](30) NULL,
        [Estado] [varchar](15) NOT NULL DEFAULT 'ACTIVO',
        [FechaCreacion] [datetime] NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_tbl_sucursales] PRIMARY KEY CLUSTERED ([SucursalId] ASC)
    );
    
    PRINT 'Tabla tbl_sucursales creada exitosamente';
END
ELSE
BEGIN
    PRINT 'Tabla tbl_sucursales ya existe';
END
GO

-- Constraint para Estado
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbl_sucursales_Estado')
BEGIN
    ALTER TABLE [dbo].[tbl_sucursales]
    ADD CONSTRAINT [CK_tbl_sucursales_Estado] 
    CHECK (([Estado]='INACTIVO' OR [Estado]='ACTIVO'));
    
    PRINT 'Constraint CK_tbl_sucursales_Estado creado';
END
GO

-- =============================================
-- PASO 2: Crear Sucursal Principal (Tienda 1)
-- =============================================
IF NOT EXISTS (SELECT 1 FROM tbl_sucursales WHERE Nombre = 'Tienda 1')
BEGIN
    INSERT INTO tbl_sucursales (Nombre, Codigo, Estado)
    VALUES ('Tienda 1', 'TIENDA001', 'ACTIVO');
    
    PRINT 'Sucursal "Tienda 1" creada';
END
ELSE
BEGIN
    PRINT 'Sucursal "Tienda 1" ya existe';
END
GO

DECLARE @SucursalPrincipalId INT;
SELECT @SucursalPrincipalId = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';
PRINT 'Sucursal Principal ID: ' + CAST(@SucursalPrincipalId AS VARCHAR(10));
GO

-- =============================================
-- PASO 3: Modificar tabla tbl_usuarios
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_usuarios]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_usuarios]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_usuarios';
END
ELSE
BEGIN
    PRINT 'Columna SucursalId ya existe en tbl_usuarios';
END
GO

-- Foreign Key
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_usuarios_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_usuarios]
    ADD CONSTRAINT [FK_usuarios_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
    
    PRINT 'Foreign Key FK_usuarios_sucursales creada';
END
ELSE
BEGIN
    PRINT 'Foreign Key FK_usuarios_sucursales ya existe';
END
GO

-- =============================================
-- PASO 4: Asignar usuarios existentes a Sucursal Principal
-- =============================================
DECLARE @SucursalPrincipalId2 INT;
SELECT @SucursalPrincipalId2 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

-- Asignar todos los usuarios (excepto SuperAdmin) a Sucursal Principal
UPDATE u
SET u.SucursalId = @SucursalPrincipalId2
FROM tbl_usuarios u
INNER JOIN tbl_roles r ON u.RolId = r.RolId
WHERE u.SucursalId IS NULL
  AND r.Nombre NOT IN ('SuperAdmin', 'SUPERADMIN', 'Super Admin');

PRINT 'Usuarios asignados a Sucursal Principal (excepto SuperAdmin)';
GO

-- =============================================
-- PASO 5: Productos son GLOBALES (NO tienen SucursalId)
-- El stock se calcula por sucursal desde tbl_inventario_movimientos
-- =============================================
PRINT 'Productos son globales - No se agrega SucursalId a tbl_productos';
PRINT 'El stock se calculará por sucursal desde tbl_inventario_movimientos';
GO

-- =============================================
-- PASO 6: Agregar SucursalId a tbl_ventas
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_ventas]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_ventas]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_ventas';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ventas_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_ventas]
    ADD CONSTRAINT [FK_ventas_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
END
GO

-- Asignar ventas existentes a Sucursal Principal
DECLARE @SucursalPrincipalId4 INT;
SELECT @SucursalPrincipalId4 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

UPDATE tbl_ventas
SET SucursalId = @SucursalPrincipalId4
WHERE SucursalId IS NULL;

PRINT 'Ventas existentes asignadas a Sucursal Principal';
GO

-- =============================================
-- PASO 7: Agregar SucursalId a tbl_compras
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_compras]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_compras]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_compras';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_compras_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_compras]
    ADD CONSTRAINT [FK_compras_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
END
GO

-- Asignar compras existentes a Sucursal Principal
DECLARE @SucursalPrincipalId5 INT;
SELECT @SucursalPrincipalId5 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

UPDATE tbl_compras
SET SucursalId = @SucursalPrincipalId5
WHERE SucursalId IS NULL;

PRINT 'Compras existentes asignadas a Sucursal Principal';
GO

-- =============================================
-- PASO 8: Agregar SucursalId a tbl_reservas
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_reservas]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_reservas]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_reservas';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_reservas_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_reservas]
    ADD CONSTRAINT [FK_reservas_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
END
GO

-- Asignar reservas existentes a Sucursal Principal
DECLARE @SucursalPrincipalId6 INT;
SELECT @SucursalPrincipalId6 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

UPDATE tbl_reservas
SET SucursalId = @SucursalPrincipalId6
WHERE SucursalId IS NULL;

PRINT 'Reservas existentes asignadas a Sucursal Principal';
GO

-- =============================================
-- PASO 9: Agregar SucursalId a tbl_caja
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_caja]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_caja]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_caja';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_caja_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_caja]
    ADD CONSTRAINT [FK_caja_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
END
GO

-- Asignar cajas existentes a Sucursal Principal
DECLARE @SucursalPrincipalId7 INT;
SELECT @SucursalPrincipalId7 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

UPDATE tbl_caja
SET SucursalId = @SucursalPrincipalId7
WHERE SucursalId IS NULL;

PRINT 'Cajas existentes asignadas a Sucursal Principal';
GO

-- =============================================
-- PASO 10: Agregar SucursalId a tbl_inventario_movimientos
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[tbl_inventario_movimientos]') AND name = 'SucursalId')
BEGIN
    ALTER TABLE [dbo].[tbl_inventario_movimientos]
    ADD [SucursalId] [int] NULL;
    
    PRINT 'Columna SucursalId agregada a tbl_inventario_movimientos';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_inventario_movimientos_sucursales')
BEGIN
    ALTER TABLE [dbo].[tbl_inventario_movimientos]
    ADD CONSTRAINT [FK_inventario_movimientos_sucursales] 
    FOREIGN KEY([SucursalId]) REFERENCES [dbo].[tbl_sucursales] ([SucursalId]);
END
GO

-- Asignar movimientos existentes a Sucursal Principal
DECLARE @SucursalPrincipalId8 INT;
SELECT @SucursalPrincipalId8 = SucursalId FROM tbl_sucursales WHERE Nombre = 'Tienda 1';

UPDATE tbl_inventario_movimientos
SET SucursalId = @SucursalPrincipalId8
WHERE SucursalId IS NULL;

PRINT 'Movimientos de inventario existentes asignados a Sucursal Principal';
GO

-- =============================================
-- PASO 11: Crear tabla para stock por producto y sucursal
-- Esta tabla permitirá ver el stock de cada producto en cada sucursal
-- =============================================
-- NOTA: El stock se calculará dinámicamente desde tbl_inventario_movimientos
-- Esta tabla es opcional y puede usarse como caché si se necesita optimización
-- Por ahora, el stock se calculará en tiempo real desde movimientos

-- =============================================
-- PASO 12: Verificar migración
-- =============================================
SELECT 
    'Sucursales creadas' AS Tipo,
    COUNT(*) AS Cantidad
FROM tbl_sucursales
UNION ALL
SELECT 
    'Usuarios con sucursal asignada',
    COUNT(*)
FROM tbl_usuarios
WHERE SucursalId IS NOT NULL
UNION ALL
-- Productos son globales, no tienen SucursalId
-- UNION ALL
-- SELECT 
--     'Productos con sucursal asignada',
--     COUNT(*)
-- FROM tbl_productos
-- WHERE SucursalId IS NOT NULL
-- UNION ALL
SELECT 
    'Ventas con sucursal asignada',
    COUNT(*)
FROM tbl_ventas
WHERE SucursalId IS NOT NULL
UNION ALL
SELECT 
    'Compras con sucursal asignada',
    COUNT(*)
FROM tbl_compras
WHERE SucursalId IS NOT NULL
UNION ALL
SELECT 
    'Reservas con sucursal asignada',
    COUNT(*)
FROM tbl_reservas
WHERE SucursalId IS NOT NULL
UNION ALL
SELECT 
    'Cajas con sucursal asignada',
    COUNT(*)
FROM tbl_caja
WHERE SucursalId IS NOT NULL
UNION ALL
SELECT 
    'Movimientos con sucursal asignada',
    COUNT(*)
FROM tbl_inventario_movimientos
WHERE SucursalId IS NOT NULL;
GO

PRINT '========================================';
PRINT 'MIGRACIÓN DE BASE DE DATOS COMPLETADA';
PRINT '========================================';
PRINT '';
PRINT 'PRÓXIMOS PASOS:';
PRINT '1. Crear stored procedures para sucursales';
PRINT '2. Crear modelos C#';
PRINT '3. Crear SucursalService';
PRINT '4. Modificar servicios existentes';
PRINT '5. Modificar controladores y vistas';
GO
