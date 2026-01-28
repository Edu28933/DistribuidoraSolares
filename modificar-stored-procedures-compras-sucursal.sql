-- =============================================
-- MODIFICAR STORED PROCEDURES DE COMPRAS
-- Agregar parámetro @SucursalId para filtrar por sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_compras_mostrar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_compras_mostrar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_compras_mostrar];
    PRINT 'Stored procedure usp_compras_mostrar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_compras_mostrar]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CompraId,
        c.Fecha,
        c.ProveedorId,
        c.UsuarioId,
        c.SucursalId,
        c.Total,
        c.Estado,
        p.Nombre AS ProveedorNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_compras c
    INNER JOIN tbl_proveedores p ON c.ProveedorId = p.ProveedorId
    INNER JOIN tbl_usuarios u ON c.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON c.SucursalId = s.SucursalId
    WHERE c.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR c.SucursalId = @SucursalId)
    ORDER BY c.Fecha DESC;
END
GO

PRINT 'Stored procedure usp_compras_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_compras_buscar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_compras_buscar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_compras_buscar];
    PRINT 'Stored procedure usp_compras_buscar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_compras_buscar]
    @Buscar VARCHAR(150),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CompraId,
        c.Fecha,
        c.ProveedorId,
        c.UsuarioId,
        c.SucursalId,
        c.Total,
        c.Estado,
        p.Nombre AS ProveedorNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_compras c
    INNER JOIN tbl_proveedores p ON c.ProveedorId = p.ProveedorId
    INNER JOIN tbl_usuarios u ON c.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON c.SucursalId = s.SucursalId
    WHERE c.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR c.SucursalId = @SucursalId)
      AND (
          CAST(c.CompraId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR p.Nombre LIKE '%' + @Buscar + '%'
          OR u.Nombre LIKE '%' + @Buscar + '%'
      )
    ORDER BY c.Fecha DESC;
END
GO

PRINT 'Stored procedure usp_compras_buscar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_compra_completa
-- Agregar parámetro @SucursalId para asignar la sucursal a la compra
-- Si @SucursalId es NULL, se obtiene del usuario
-- =============================================
-- NOTA: Este stored procedure es complejo y maneja la creación completa de la compra
-- Necesitamos agregar @SucursalId como parámetro opcional y usarlo al insertar en tbl_compras
-- Si @SucursalId es NULL, obtenerlo de tbl_usuarios donde UsuarioId = @UsuarioId
-- 
-- IMPORTANTE: Este script asume que el stored procedure tiene una estructura similar a:
--   INSERT INTO tbl_compras (..., SucursalId, ...) VALUES (..., @SucursalIdCalculado, ...)
-- 
-- Si el stored procedure actual no tiene esta estructura, será necesario revisarlo manualmente
PRINT 'NOTA: usp_compra_completa necesita ser modificado manualmente para incluir @SucursalId';
PRINT 'El stored procedure debe:';
PRINT '  1. Aceptar @SucursalId INT = NULL como parámetro';
PRINT '  2. Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId';
PRINT '  3. Usar ese SucursalId al insertar en tbl_compras';
PRINT '  4. También usar ese SucursalId al crear movimientos de inventario';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE COMPRAS MODIFICADOS';
PRINT '========================================';
PRINT 'usp_compras_mostrar: ✓ Modificado';
PRINT 'usp_compras_buscar: ✓ Modificado';
PRINT 'usp_compra_completa: ⚠️ Requiere modificación manual';
PRINT '========================================';
GO
