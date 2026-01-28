-- =============================================
-- MODIFICAR STORED PROCEDURES DE CAJA
-- Agregar parámetro @SucursalId para filtrar por sucursal
-- =============================================

USE [bd_distribuidora_solares];
GO

-- =============================================
-- Modificar usp_caja_mostrar
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[usp_caja_mostrar]') AND type in (N'P', N'PC'))
BEGIN
    DROP PROCEDURE [dbo].[usp_caja_mostrar];
    PRINT 'Stored procedure usp_caja_mostrar eliminado para modificación';
END
GO

CREATE PROCEDURE [dbo].[usp_caja_mostrar]
    @SucursalId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.CajaId,
        c.SucursalId,
        c.FechaApertura,
        c.SaldoInicial,
        c.FechaCierre,
        c.SaldoFinal,
        c.Estado,
        s.Nombre AS SucursalNombre
    FROM tbl_caja c
    LEFT JOIN tbl_sucursales s ON c.SucursalId = s.SucursalId
    WHERE (@SucursalId IS NULL OR c.SucursalId = @SucursalId)
    ORDER BY c.FechaApertura DESC;
END
GO

PRINT 'Stored procedure usp_caja_mostrar modificado exitosamente';
GO

-- =============================================
-- Modificar usp_caja_abrir
-- Agregar parámetro @SucursalId para asignar la sucursal a la caja
-- Si @SucursalId es NULL, se obtiene del usuario
-- =============================================
-- NOTA: Este stored procedure necesita ser modificado manualmente
-- Debe aceptar @SucursalId INT = NULL y usarlo al insertar en tbl_caja
-- Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId
PRINT 'NOTA: usp_caja_abrir necesita ser modificado manualmente para incluir @SucursalId';
PRINT 'El stored procedure debe:';
PRINT '  1. Aceptar @SucursalId INT = NULL como parámetro';
PRINT '  2. Si @SucursalId IS NULL, obtenerlo de: SELECT SucursalId FROM tbl_usuarios WHERE UsuarioId = @UsuarioId';
PRINT '  3. Usar ese SucursalId al insertar en tbl_caja';
GO

PRINT '========================================';
PRINT 'STORED PROCEDURES DE CAJA MODIFICADOS';
PRINT '========================================';
PRINT 'usp_caja_mostrar: ✓ Modificado';
PRINT 'usp_caja_abrir: ⚠️ Requiere modificación manual';
PRINT '========================================';
GO
