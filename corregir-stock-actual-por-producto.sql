-- =============================================
-- CORREGIR usp_stock_actual_por_producto
-- Agregar StockMinimo que falta en el SELECT
-- =============================================

USE [bd_distribuidora_solares];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_actual_por_producto]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_stock_actual_por_producto];
    PRINT 'Stored procedure usp_stock_actual_por_producto eliminado para corrección';
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

PRINT 'Stored procedure usp_stock_actual_por_producto corregido exitosamente';
PRINT 'Ahora incluye StockMinimo en el resultado';
GO
