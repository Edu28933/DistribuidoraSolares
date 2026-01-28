-- =============================================
-- MODIFICAR STORED PROCEDURES DE INVENTARIO MOVIMIENTOS
-- Agregar parámetro @SucursalId para filtrar por sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_inventario_movimientos_mostrar
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
        im.CostoUnitario,
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
    WHERE im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_buscar
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
        im.CostoUnitario,
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
    WHERE im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
      AND (
          CAST(im.InvMovId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR p.Nombre LIKE '%' + @Buscar + '%'
          OR im.Tipo LIKE '%' + @Buscar + '%'
          OR u.Nombre LIKE '%' + @Buscar + '%'
      )
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_buscar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_por_producto
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
        im.CostoUnitario,
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
    WHERE im.ProductoId = @ProductoId
      AND im.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR im.SucursalId = @SucursalId)
    ORDER BY im.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_inventario_movimientos_por_producto modificado exitosamente';
GO

-- =============================================
-- Modificar usp_inventario_movimientos_crear
-- Agregar parámetro @SucursalId para asignar la sucursal al movimiento
-- Si @SucursalId es NULL, se obtiene del usuario
-- =============================================
-- NOTA: Este stored procedure necesita ser modificado manualmente
-- Debe aceptar @SucursalId INT = NULL y usarlo al insertar en tbl_inventario_movimientos
-- Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId
PRINT 'NOTA: usp_inventario_movimientos_crear necesita ser modificado manualmente para incluir @SucursalId';
PRINT 'El stored procedure debe:';
PRINT '  1. Aceptar @SucursalId INT = NULL como parámetro';
PRINT '  2. Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId';
PRINT '  3. Usar ese SucursalId al insertar en tbl_inventario_movimientos';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE INVENTARIO MOVIMIENTOS MODIFICADOS';
PRINT '========================================';
PRINT 'usp_inventario_movimientos_mostrar: ✓ Modificado';
PRINT 'usp_inventario_movimientos_buscar: ✓ Modificado';
PRINT 'usp_inventario_movimientos_por_producto: ✓ Modificado';
PRINT 'usp_inventario_movimientos_crear: ⚠️ Requiere modificación manual';
PRINT '========================================';
GO
