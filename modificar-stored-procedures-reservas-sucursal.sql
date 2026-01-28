-- =============================================
-- MODIFICAR STORED PROCEDURES DE RESERVAS
-- Agregar parámetro @SucursalId para filtrar por sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_reservas_mostrar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_reservas_mostrar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_reservas_mostrar];
    PRINT 'Stored procedure usp_reservas_mostrar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_reservas_mostrar]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.ReservaId,
        r.ClienteId,
        r.UsuarioId,
        r.SucursalId,
        r.FechaHora,
        r.FechaVencimiento,
        r.Estado,
        r.Observacion,
        c.Nombre AS ClienteNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_reservas r
    INNER JOIN tbl_clientes c ON r.ClienteId = c.ClienteId
    INNER JOIN tbl_usuarios u ON r.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON r.SucursalId = s.SucursalId
    WHERE r.Estado IN ('ACTIVA', 'CONVERTIDA', 'CANCELADA')
      AND (@SucursalId IS NULL OR r.SucursalId = @SucursalId)
    ORDER BY r.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_reservas_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_reservas_buscar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_reservas_buscar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_reservas_buscar];
    PRINT 'Stored procedure usp_reservas_buscar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_reservas_buscar]
    @Buscar VARCHAR(150),
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        r.ReservaId,
        r.ClienteId,
        r.UsuarioId,
        r.SucursalId,
        r.FechaHora,
        r.FechaVencimiento,
        r.Estado,
        r.Observacion,
        c.Nombre AS ClienteNombre,
        u.Nombre AS UsuarioNombre,
        s.Nombre AS SucursalNombre
    FROM tbl_reservas r
    INNER JOIN tbl_clientes c ON r.ClienteId = c.ClienteId
    INNER JOIN tbl_usuarios u ON r.UsuarioId = u.UsuarioId
    LEFT JOIN tbl_sucursales s ON r.SucursalId = s.SucursalId
    WHERE r.Estado IN ('ACTIVA', 'CONVERTIDA', 'CANCELADA')
      AND (@SucursalId IS NULL OR r.SucursalId = @SucursalId)
      AND (
          CAST(r.ReservaId AS VARCHAR(10)) LIKE '%' + @Buscar + '%'
          OR c.Nombre LIKE '%' + @Buscar + '%'
          OR u.Nombre LIKE '%' + @Buscar + '%'
      )
    ORDER BY r.FechaHora DESC;
END
GO

PRINT 'Stored procedure usp_reservas_buscar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_reserva_crear_completa
-- Agregar parámetro @SucursalId para asignar la sucursal a la reserva
-- Si @SucursalId es NULL, se obtiene del usuario
-- =============================================
-- NOTA: Este stored procedure es complejo y maneja la creación completa de la reserva
-- Necesitamos agregar @SucursalId como parámetro opcional y usarlo al insertar en tbl_reservas
-- Si @SucursalId es NULL, obtenerlo de tbl_usuarios donde UsuarioId = @UsuarioId
-- 
-- IMPORTANTE: Este script asume que el stored procedure tiene una estructura similar a:
--   INSERT INTO tbl_reservas (..., SucursalId, ...) VALUES (..., @SucursalIdCalculado, ...)
-- 
-- Si el stored procedure actual no tiene esta estructura, será necesario revisarlo manualmente
PRINT 'NOTA: usp_reserva_crear_completa necesita ser modificado manualmente para incluir @SucursalId';
PRINT 'El stored procedure debe:';
PRINT '  1. Aceptar @SucursalId INT = NULL como parámetro';
PRINT '  2. Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId';
PRINT '  3. Usar ese SucursalId al insertar en tbl_reservas';
PRINT '  4. También usar ese SucursalId al crear movimientos de inventario';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE RESERVAS MODIFICADOS';
PRINT '========================================';
PRINT 'usp_reservas_mostrar: ✓ Modificado';
PRINT 'usp_reservas_buscar: ✓ Modificado';
PRINT 'usp_reserva_crear_completa: ⚠️ Requiere modificación manual';
PRINT '========================================';
GO
