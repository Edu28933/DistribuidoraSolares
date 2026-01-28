-- =============================================
-- FIX STOCK: Transferencias + Stock por Sucursal
-- Ejecutar en bd_distribuidora_solares.
-- 1) usp_stock_actual_por_producto devuelve StockMinimo -> las transferencias dejan de fallar.
-- 2) Se rellenan movimientos con SucursalId NULL desde venta/compra/sucursal por defecto -> Stock por Sucursal coincide con el total real.
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- PARTE 1: usp_stock_actual_por_producto con StockMinimo (para transferencias)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_actual_por_producto]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_stock_actual_por_producto];
GO

CREATE PROCEDURE [dbo].[usp_stock_actual_por_producto]
    @ProductoId INT,
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.ProductoId,
        p.Codigo,
        p.Nombre,
        p.StockMinimo,
        StockActual = ISNULL(SUM(CASE WHEN im.Estado = 'ACTIVO' THEN im.Cantidad ELSE 0 END), 0)
    FROM dbo.tbl_productos p
    LEFT JOIN dbo.tbl_inventario_movimientos im
        ON im.ProductoId = p.ProductoId
        AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
    WHERE p.ProductoId = @ProductoId
    GROUP BY p.ProductoId, p.Codigo, p.Nombre, p.StockMinimo;
END
GO

PRINT '1. usp_stock_actual_por_producto actualizado (incluye StockMinimo).';
GO

-- =============================================
-- PARTE 2: Completar SucursalId en movimientos NULL
-- (VentaId/CompraId -> sucursal; si no, sucursal por defecto)
-- =============================================
DECLARE @SucursalPorDefecto INT;
SELECT TOP 1 @SucursalPorDefecto = SucursalId
FROM dbo.tbl_sucursales
WHERE Estado = 'ACTIVO'
ORDER BY SucursalId;

-- Desde venta
UPDATE im
SET im.SucursalId = v.SucursalId
FROM dbo.tbl_inventario_movimientos im
INNER JOIN dbo.tbl_ventas v ON v.VentaId = im.VentaId
WHERE im.SucursalId IS NULL AND v.SucursalId IS NOT NULL;

-- Desde compra
UPDATE im
SET im.SucursalId = c.SucursalId
FROM dbo.tbl_inventario_movimientos im
INNER JOIN dbo.tbl_compras c ON c.CompraId = im.CompraId
WHERE im.SucursalId IS NULL AND c.SucursalId IS NOT NULL;

-- Los que sigan en NULL -> sucursal por defecto
IF @SucursalPorDefecto IS NOT NULL
BEGIN
    UPDATE dbo.tbl_inventario_movimientos
    SET SucursalId = @SucursalPorDefecto
    WHERE SucursalId IS NULL;
END

PRINT '2. SucursalId completado en movimientos que lo tenían NULL.';
GO

-- =============================================
-- PARTE 3: usp_stock_general_por_sucursal (stock real por sucursal)
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_general_por_sucursal]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_stock_general_por_sucursal];
GO

CREATE PROCEDURE [dbo].[usp_stock_general_por_sucursal]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @SucursalId IS NOT NULL
    BEGIN
        SELECT 
            p.ProductoId,
            p.Codigo,
            p.Nombre,
            p.StockMinimo,
            @SucursalId AS SucursalId,
            s.Nombre AS SucursalNombre,
            StockActual = ISNULL(SUM(CASE WHEN im.Estado = 'ACTIVO' THEN im.Cantidad ELSE 0 END), 0)
        FROM dbo.tbl_productos p
        CROSS JOIN dbo.tbl_sucursales s
        LEFT JOIN dbo.tbl_inventario_movimientos im 
            ON im.ProductoId = p.ProductoId 
            AND im.SucursalId = @SucursalId
            AND im.Estado = 'ACTIVO'
        WHERE s.SucursalId = @SucursalId
          AND s.Estado = 'ACTIVO'
        GROUP BY p.ProductoId, p.Codigo, p.Nombre, p.StockMinimo, s.SucursalId, s.Nombre
        ORDER BY p.Nombre;
    END
    ELSE
    BEGIN
        SELECT 
            p.ProductoId,
            p.Codigo,
            p.Nombre,
            p.StockMinimo,
            s.SucursalId,
            s.Nombre AS SucursalNombre,
            StockActual = ISNULL(SUM(CASE WHEN im.Estado = 'ACTIVO' THEN im.Cantidad ELSE 0 END), 0)
        FROM dbo.tbl_productos p
        CROSS JOIN dbo.tbl_sucursales s
        LEFT JOIN dbo.tbl_inventario_movimientos im 
            ON im.ProductoId = p.ProductoId 
            AND im.SucursalId = s.SucursalId
            AND im.Estado = 'ACTIVO'
        WHERE s.Estado = 'ACTIVO'
        GROUP BY p.ProductoId, p.Codigo, p.Nombre, p.StockMinimo, s.SucursalId, s.Nombre
        ORDER BY p.Nombre, s.Nombre;
    END
END
GO

PRINT '3. usp_stock_general_por_sucursal actualizado.';
PRINT '';
PRINT 'Listo: transferencias usarán StockMinimo y Stock por Sucursal mostrará el stock real.';
GO
