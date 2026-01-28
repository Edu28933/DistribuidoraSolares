-- =============================================
-- MODIFICAR STORED PROCEDURES DE STOCK
-- Agregar filtro por @SucursalId
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_stock_actual_general
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_actual_general]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_stock_actual_general];
    PRINT 'Stored procedure usp_stock_actual_general eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_stock_actual_general]
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
    GROUP BY p.ProductoId, p.Codigo, p.Nombre, p.StockMinimo
    ORDER BY p.Nombre;
END
GO

PRINT 'Stored procedure usp_stock_actual_general modificado exitosamente';
GO

-- =============================================
-- Modificar usp_stock_actual_por_producto
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_actual_por_producto]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_stock_actual_por_producto];
    PRINT 'Stored procedure usp_stock_actual_por_producto eliminado para modificación';
END
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

PRINT 'Stored procedure usp_stock_actual_por_producto modificado exitosamente (incluye StockMinimo)';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE STOCK MODIFICADOS';
PRINT '========================================';
PRINT 'usp_stock_actual_general: ✓ Modificado';
PRINT 'usp_stock_actual_por_producto: ✓ Modificado';
PRINT '========================================';
GO
