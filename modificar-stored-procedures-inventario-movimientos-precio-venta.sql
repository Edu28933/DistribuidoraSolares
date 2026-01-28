-- =============================================
-- MODIFICAR STORED PROCEDURES DE INVENTARIO MOVIMIENTOS
-- Agregar precio de venta desde tbl_venta_detalle para mostrar valor total de ventas
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_inventario_movimientos_mostrar
-- Agregar PrecioUnitarioVenta desde tbl_venta_detalle
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_inventario_movimientos_mostrar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_inventario_movimientos_mostrar];
    PRINT 'Stored procedure usp_inventario_movimientos_mostrar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_inventario_movimientos_mostrar]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        im.InvMovId,
        im.ProductoId,
        im.SucursalId,
        im.FechaHora,
        im.Tipo,
        im.Cantidad,
        -- Para VENTA: obtener precio desde tbl_venta_detalle
        -- Para COMPRA: usar CostoUnitario de tbl_inventario_movimientos
        CASE 
            WHEN im.Tipo = 'VENTA' AND im.VentaId IS NOT NULL THEN vd.PrecioUnitario
            ELSE im.CostoUnitario
        END AS CostoUnitario,
        im.VentaId,
        im.CompraId,
        im.ReservaId,
        im.Descripcion,
        im.UsuarioId,
        im.Estado,
        p.Nombre AS ProductoNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_inventario_movimientos im
    INNER JOIN tbl_productos p ON im.ProductoId = p.ProductoId
    INNER JOIN tbl_usuarios u ON im.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON im.SucursalId = s.SucursalId
    LEFT JOIN tbl_venta_detalle vd ON im.VentaId = vd.VentaId AND im.ProductoId = vd.ProductoId
    WHERE im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_buscar
-- Agregar PrecioUnitarioVenta desde tbl_venta_detalle
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_inventario_movimientos_buscar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_inventario_movimientos_buscar];
    PRINT 'Stored procedure usp_inventario_movimientos_buscar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_inventario_movimientos_buscar]
    @Buscar VARCHAR(150),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        im.InvMovId,
        im.ProductoId,
        im.SucursalId,
        im.FechaHora,
        im.Tipo,
        im.Cantidad,
        -- Para VENTA: obtener precio desde tbl_venta_detalle
        -- Para COMPRA: usar CostoUnitario de tbl_inventario_movimientos
        CASE 
            WHEN im.Tipo = 'VENTA' AND im.VentaId IS NOT NULL THEN vd.PrecioUnitario
            ELSE im.CostoUnitario
        END AS CostoUnitario,
        im.VentaId,
        im.CompraId,
        im.ReservaId,
        im.Descripcion,
        im.UsuarioId,
        im.Estado,
        p.Nombre AS ProductoNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_inventario_movimientos im
    INNER JOIN tbl_productos p ON im.ProductoId = p.ProductoId
    INNER JOIN tbl_usuarios u ON im.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON im.SucursalId = s.SucursalId
    LEFT JOIN tbl_venta_detalle vd ON im.VentaId = vd.VentaId AND im.ProductoId = vd.ProductoId
    WHERE im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
      AND (
          p.Nombre LIKE '%' + @Buscar + '%'
          OR CAST(im.InvMovId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR CAST(im.VentaId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR CAST(im.CompraId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
      )
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_buscar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_por_producto
-- Agregar PrecioUnitarioVenta desde tbl_venta_detalle
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_inventario_movimientos_por_producto]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_inventario_movimientos_por_producto];
    PRINT 'Stored procedure usp_inventario_movimientos_por_producto eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_inventario_movimientos_por_producto]
    @ProductoId INT,
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        im.InvMovId,
        im.ProductoId,
        im.SucursalId,
        im.FechaHora,
        im.Tipo,
        im.Cantidad,
        -- Para VENTA: obtener precio desde tbl_venta_detalle
        -- Para COMPRA: usar CostoUnitario de tbl_inventario_movimientos
        CASE 
            WHEN im.Tipo = 'VENTA' AND im.VentaId IS NOT NULL THEN vd.PrecioUnitario
            ELSE im.CostoUnitario
        END AS CostoUnitario,
        im.VentaId,
        im.CompraId,
        im.ReservaId,
        im.Descripcion,
        im.UsuarioId,
        im.Estado,
        p.Nombre AS ProductoNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_inventario_movimientos im
    INNER JOIN tbl_productos p ON im.ProductoId = p.ProductoId
    INNER JOIN tbl_usuarios u ON im.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON im.SucursalId = s.SucursalId
    LEFT JOIN tbl_venta_detalle vd ON im.VentaId = vd.VentaId AND im.ProductoId = vd.ProductoId
    WHERE im.ProductoId = @ProductoId
      AND im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_por_producto modificado exitosamente';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE INVENTARIO MOVIMIENTOS MODIFICADOS';
PRINT 'Ahora incluyen precio de venta desde tbl_venta_detalle';
PRINT '========================================';
GO
