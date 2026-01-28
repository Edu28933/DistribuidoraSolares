-- =============================================
-- STORED PROCEDURE: Stock por Producto y Sucursal
-- Permite ver el stock de un producto en todas las sucursales
-- =============================================

USE [bd_distribuidora_solares];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_por_producto_sucursal]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_stock_por_producto_sucursal];
GO

CREATE PROCEDURE [dbo].[usp_stock_por_producto_sucursal]
    @ProductoId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Calcular stock por sucursal para un producto específico
    SELECT 
        s.SucursalId,
        s.Nombre AS SucursalNombre,
        s.Codigo AS SucursalCodigo,
        ISNULL(SUM(CASE WHEN im.Estado = 'ACTIVO' THEN im.Cantidad ELSE 0 END), 0) AS StockActual
    FROM dbo.tbl_sucursales s
    LEFT JOIN dbo.tbl_inventario_movimientos im 
        ON im.SucursalId = s.SucursalId 
        AND im.ProductoId = @ProductoId
    WHERE s.Estado = 'ACTIVO'
    GROUP BY s.SucursalId, s.Nombre, s.Codigo
    ORDER BY s.Nombre;
END
GO

-- =============================================
-- STORED PROCEDURE: Stock General por Sucursal
-- Muestra todos los productos con su stock real en cada sucursal.
-- Misma lógica que usp_stock_actual_por_producto: SUM(Cantidad) de movimientos ACTIVO
-- (COMPRA +, VENTA/PERDIDA -, AJUSTE/RESERVA/LIBERACION según signo).
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_stock_general_por_sucursal]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[usp_stock_general_por_sucursal];
GO

CREATE PROCEDURE [dbo].[usp_stock_general_por_sucursal]
    @SucursalId INT = NULL  -- NULL = todas las sucursales
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @SucursalId IS NOT NULL
    BEGIN
        -- Stock real de una sucursal: solo movimientos de esa sucursal
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
        -- Stock real por cada sucursal: movimientos agrupados por (producto, sucursal)
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

PRINT 'Stored procedures de stock por sucursal creados exitosamente';
GO
