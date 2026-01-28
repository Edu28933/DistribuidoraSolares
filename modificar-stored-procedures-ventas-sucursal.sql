-- =============================================
-- MODIFICAR STORED PROCEDURES DE VENTAS
-- Agregar parámetro @SucursalId para filtrar por sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_ventas_mostrar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ventas_mostrar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_ventas_mostrar];
    PRINT 'Stored procedure usp_ventas_mostrar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_ventas_mostrar]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VentaId,
        v.FechaHora,
        v.ClienteId,
        v.UsuarioId,
        v.MetodoPagoId,
        v.SucursalId,
        v.TipoVenta,
        v.TotalBruto,
        v.DescuentoManual,
        v.TotalNeto,
        v.Observacion,
        v.Estado,
        c.Nombre AS ClienteNombre,
        u.Nombre AS UsuarioNombre,
        mp.Nombre AS MetodoPagoNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_ventas v
    LEFT JOIN tbl_clientes c ON v.ClienteId = c.ClienteId
    INNER JOIN tbl_usuarios u ON v.UsuarioId = u.UsuarioId
    INNER JOIN tbl_metodos_pago mp ON v.MetodoPagoId = mp.MetodoPagoId
    LEFT JOIN tbl_sucursales s ON v.SucursalId = s.SucursalId
    WHERE v.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
    ORDER BY v.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_ventas_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_ventas_buscar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_ventas_buscar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_ventas_buscar];
    PRINT 'Stored procedure usp_ventas_buscar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_ventas_buscar]
    @Buscar VARCHAR(150),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VentaId,
        v.FechaHora,
        v.ClienteId,
        v.UsuarioId,
        v.MetodoPagoId,
        v.SucursalId,
        v.TipoVenta,
        v.TotalBruto,
        v.DescuentoManual,
        v.TotalNeto,
        v.Observacion,
        v.Estado,
        c.Nombre AS ClienteNombre,
        u.Nombre AS UsuarioNombre,
        mp.Nombre AS MetodoPagoNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_ventas v
    LEFT JOIN tbl_clientes c ON v.ClienteId = c.ClienteId
    INNER JOIN tbl_usuarios u ON v.UsuarioId = u.UsuarioId
    INNER JOIN tbl_metodos_pago mp ON v.MetodoPagoId = mp.MetodoPagoId
    LEFT JOIN tbl_sucursales s ON v.SucursalId = s.SucursalId
    WHERE v.Estado = 'ACTIVO'
      AND (@SucursalId IS NULL OR v.SucursalId = @SucursalId)
      AND (
          CAST(v.VentaId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR c.Nombre LIKE '%' + @Buscar + '%'
          OR u.Nombre LIKE '%' + @Buscar + '%'
          OR mp.Nombre LIKE '%' + @Buscar + '%'
      )
    ORDER BY v.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_ventas_buscar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_venta_completa
-- Agregar parámetro @SucursalId para asignar la sucursal a la venta
-- Si @SucursalId es NULL, se obtiene del usuario
-- =============================================
-- NOTA: Este stored procedure es complejo y maneja la creación completa de la venta
-- Necesitamos agregar @SucursalId como parámetro opcional y usarlo al insertar en tbl_ventas
-- Si @SucursalId es NULL, obtenerlo de tbl_usuarios donde UsuarioId = @UsuarioId
-- 
-- IMPORTANTE: Este script asume que el stored procedure tiene una estructura similar a:
--   INSERT INTO tbl_ventas (..., SucursalId, ...) VALUES (..., @SucursalIdCalculado, ...)
-- 
-- Si el stored procedure actual no tiene esta estructura, será necesario revisarlo manualmente
PRINT 'NOTA: usp_venta_completa necesita ser modificado manualmente para incluir @SucursalId';
PRINT 'El stored procedure debe:';
PRINT '  1. Aceptar @SucursalId INT = NULL como parámetro';
PRINT '  2. Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId';
PRINT '  3. Usar ese SucursalId al insertar en tbl_ventas';
PRINT '  4. También usar ese SucursalId al crear movimientos de inventario';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE VENTAS MODIFICADOS';
PRINT '========================================';
PRINT 'usp_ventas_mostrar: ✓ Modificado';
PRINT 'usp_ventas_buscar: ✓ Modificado';
PRINT 'usp_venta_completa: ⚠️ Requiere modificación manual';
PRINT '========================================';
GO
