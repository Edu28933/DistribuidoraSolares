-- =============================================
-- ACTUALIZAR usp_stock_general_por_sucursal
-- Ejecutar en la BD para que "Stock por Sucursal" muestre el stock real.
-- Misma lógica que usp_stock_actual_por_producto: SUM(Cantidad) de movimientos ACTIVO.
-- =============================================

USE [bd_distribuidora_solares];
GO

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

PRINT 'usp_stock_general_por_sucursal actualizado. Stock por sucursal usará stock real (SUM movimientos ACTIVO).';
GO
